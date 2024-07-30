using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Maps;
using Eurofurence.App.Server.Services.Abstractions.Maps;
using Eurofurence.App.Server.Services.Abstractions.Validation;
using Eurofurence.App.Server.Web.Extensions;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eurofurence.App.Server.Web.Controllers
{
    /// <summary>
    /// FindMe1
    /// </summary>
    [Route("Api/[controller]")]
    public class MapsController : BaseController
    {
        private readonly ILinkFragmentValidator _linkFragmentValidator;
        private readonly IMapService _mapService;
        private readonly IMapper _mapper;

        public MapsController(IMapService mapService, ILinkFragmentValidator linkFragmentValidator, IMapper mapper)
        {
            _mapService = mapService;
            _linkFragmentValidator = linkFragmentValidator;
            _mapper = mapper;
        }

        /// <summary>
        ///     Get all maps
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<MapResponse>), 200)]
        public IEnumerable<MapResponse> GetMapsAsync()
        {
            return _mapper.Map<IEnumerable<MapResponse>>(_mapService.FindAll());
        }

        /// <summary>
        ///     Get a specific map
        /// </summary>
        /// <response code="404">
        ///     * No map found for `id`
        /// </response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MapResponse), 200)]
        public async Task<MapResponse> GetMapAsync([FromRoute] Guid id)
        {
            return _mapper.Map<MapResponse>(await _mapService.FindOneAsync(id).Transient404(HttpContext));
        }


        /// <summary>
        ///     Get all map entries for a specific map
        /// </summary>
        /// <response code="404">
        ///     * No map found for `id`
        /// </response>
        [HttpGet("{id}/Entries")]
        [ProducesResponseType(typeof(IEnumerable<MapEntryResponse>), 200)]
        public async Task<IEnumerable<MapEntryResponse>> GetMapEntriesAsync([FromRoute] Guid id)
        {
            return _mapper.Map<MapResponse>(await _mapService.FindOneAsync(id).Transient404(HttpContext))?.Entries;
        }

        /// <summary>
        ///     Get all specific map entry for a specific map
        /// </summary>
        /// <response code="404">
        ///     * No map found for `id`
        ///     * No entry found for `entryId`on map
        /// </response>
        [HttpGet("{id}/Entries/{entryId}")]
        [ProducesResponseType(typeof(MapEntryResponse), 200)]
        public async Task<MapEntryResponse> GetSingleMapEntryAsync([FromRoute] Guid id, [FromRoute] Guid entryId)
        {
            var result = (await _mapService.FindOneAsync(id))?.Entries.SingleOrDefault(a => a.Id == entryId).Transient404(HttpContext);

            return result != null ? _mapper.Map<MapEntryResponse>(result) : null;
        }

        /// <summary>
        ///     Create a new map.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Id of the newly created map</returns>
        [Authorize(Roles = "Admin,Developer")]
        [ProducesResponseType(typeof(Guid), 200)]
        [HttpPost("")]
        public async Task<ActionResult> PostMapAsync(
            [EnsureNotNull][FromBody] MapRequest request
        )
        {
            var record = _mapper.Map<MapRecord>(request);
            await _mapService.InsertOneAsync(record);
            return Ok(record.Id);
        }

        /// <summary>
        ///     Delete a map.
        /// </summary>
        /// <param name="id"></param>
        [Authorize(Roles = "Admin,Developer")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 404)]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMapAsync(
            [EnsureNotNull][FromRoute] Guid id
        )
        {
            var existingRecord = await _mapService.FindOneAsync(id);
            if (existingRecord == null) return NotFound($"No record found with it {id}");

            await _mapService.DeleteOneAsync(id);

            return NoContent();
        }

        /// <summary>
        ///     Delete all map entries for a specific map
        /// </summary>
        /// <response code="400">
        ///     * Unable to parse `id`
        ///     * No map found for the given `id`
        /// </response>
        [HttpDelete("{id}/Entries")]
        [Authorize(Roles = "Admin,Developer")]
        [ProducesResponseType(204)]
        public async Task<ActionResult> DeleteMapEntriesAsync([FromRoute] Guid id)
        {
            if (id == Guid.Empty) return BadRequest("Error parsing Id");

            var map = await _mapService.FindOneAsync(id);
            if (map == null) return BadRequest("No map with this id");

            await _mapService.DeleteAllEntriesAsync(id);
            return NoContent();
        }

        /// <summary>
        ///     Delete a specific map entry for a specific map
        /// </summary>
        /// <response code="404">No entry found on the map for the given `entryId`</response>
        /// <response code="400">
        ///     * Unable to parse `id` or `entryId`
        ///     * No map found for the given `id`
        /// </response>
        [HttpDelete("{id}/Entries/{entryId}")]
        [Authorize(Roles = "Admin,Developer")]
        [ProducesResponseType(204)]
        public async Task<ActionResult> DeleteSingleMapEntryAsync([FromRoute] Guid id, [FromRoute] Guid entryId)
        {
            if (id == Guid.Empty) return BadRequest("Error parsing Id");
            if (entryId == Guid.Empty) return BadRequest("Error parsing EntryId");

            var map = await _mapService.FindOneAsync(id);
            if (map == null) return BadRequest("No map with this id");

            var entry = map.Entries.SingleOrDefault(a => a.Id == entryId);
            if (entry == null) return NotFound();

            await _mapService.DeleteOneEntryAsync(entryId);

            return NoContent();
        }

        /// <summary>
        ///     Create a new map entry in a specific map
        /// </summary>
        /// <remarks>If you can generate guids client-side, you can also use the PUT variant for both create and update.</remarks>
        /// <param name="request">Do not specify the "Id" property. It will be auto-assigned and returned in the response.</param>
        /// <param name="id">"Id" of the map</param>
        /// <returns>The id of the new map entry (guid)</returns>
        /// <response code="400">
        ///     * Unable to parse `record` or `id`
        /// </response>
        [HttpPost("{id}/Entries")]
        [Authorize(Roles = "Admin,Developer")]
        [ProducesResponseType(typeof(Guid), 200)]
        public async Task<ActionResult> PostSingleMapEntryAsync([FromBody] MapEntryRequest request, [FromRoute] Guid id)
        {
            if (request == null) return BadRequest("Error parsing request");
            if (id == Guid.Empty) return BadRequest("Error parsing Id");

            var record = _mapper.Map<MapEntryRecord>(request);
            record.MapId = id;
            await _mapService.InsertOneEntryAsync(record);
            return Ok(record.Id);
        }

        /// <summary>
        ///     Create or Update an existing map entry in a specific map
        /// </summary>
        /// <remarks>
        ///     The id property of the model (request body) is optional, but if provided, it must match the {entryId} part of the uri.
        /// </remarks>
        /// <param name="request">"Id" property must match the {EntryId} part of the uri</param>
        /// <param name="id">"Id" of the map.</param>
        /// <param name="entryId">"Id" of the entry that gets inserted.</param>
        /// <response code="400">
        ///     * Unable to parse `record`, `id` or `entryId`
        ///     * `record.Id` does not match `entryId` from uri.
        ///     * No map found with for the specified id.
        /// </response>
        [HttpPut("{id}/Entries/{entryId}")]
        [Authorize(Roles = "Admin,Developer")]
        [ProducesResponseType(typeof(Guid), 200)]
        public async Task<ActionResult> PutSingleMapEntryAsync([FromBody] MapEntryRequest request, [FromRoute] Guid id,
            [FromRoute] Guid entryId)
        {
            if (request == null) return BadRequest("Error parsing Record");
            if (id == Guid.Empty) return BadRequest("Error parsing Id");
            if (entryId == Guid.Empty) return BadRequest("Error parsing EntryId");
            if (request.Id != Guid.Empty && request.Id != entryId) return BadRequest("EntityId must match Record.Id");

            var map = await _mapService.FindOneAsync(id);
            if (map == null) return BadRequest("No map with this id");

            foreach (var link in request.Links)
            {
                var linkValidation = await _linkFragmentValidator.ValidateAsync(link);
                if (!linkValidation.IsValid) return BadRequest(linkValidation.ErrorMessage);
            }

            var record = _mapper.Map<MapEntryRecord>(request);
            record.Id = entryId;
            record.MapId = id;
            await _mapService.ReplaceOneEntryAsync(record);
            return Ok(record.Id);
        }
    }
}