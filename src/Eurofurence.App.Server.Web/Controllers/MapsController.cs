using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Maps;
using Eurofurence.App.Server.Services.Abstractions.Maps;
using Eurofurence.App.Server.Services.Abstractions.Validation;
using Eurofurence.App.Server.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/v2/[controller]")]
    public class MapsController : Controller
    {
        private readonly ILinkFragmentValidator _linkFragmentValidator;
        private readonly IMapService _mapService;

        public MapsController(IMapService mapService, ILinkFragmentValidator linkFragmentValidator)
        {
            _mapService = mapService;
            _linkFragmentValidator = linkFragmentValidator;
        }

        /// <summary>
        ///     Get all maps
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<MapRecord>), 200)]
        public Task<IEnumerable<MapRecord>> GetMapsAsync()
        {
            return _mapService.FindAllAsync();
        }

        /// <summary>
        ///     Get a specific map
        /// </summary>
        /// <response code="404">
        ///     * No map found for `Id`
        /// </response>
        [HttpGet("{Id}")]
        [ProducesResponseType(typeof(MapRecord), 200)]
        public async Task<MapRecord> GetMapAsync([FromRoute] Guid Id)
        {
            return (await _mapService.FindOneAsync(Id)).Transient404(HttpContext);
        }


        /// <summary>
        ///     Get all map entries for a specific map
        /// </summary>
        /// <response code="404">
        ///     * No map found for `Id`
        /// </response>
        [HttpGet("{Id}/Entries")]
        [ProducesResponseType(typeof(ICollection<MapEntryRecord>), 200)]
        public async Task<ICollection<MapEntryRecord>> GetMapEntriesAsync([FromRoute] Guid Id)
        {
            return (await _mapService.FindOneAsync(Id)).Transient404(HttpContext)?.Entries;
        }

        /// <summary>
        ///     Get all specific map entry for a specific map
        /// </summary>
        /// <response code="404">
        ///     * No map found for `Id`
        ///     * No entry found for `EntryId`on map
        /// </response>
        [HttpGet("{Id}/Entries/{EntryId}")]
        [ProducesResponseType(typeof(MapEntryRecord), 200)]
        public async Task<MapEntryRecord> GetSingleMapEntryAsync([FromRoute] Guid Id, [FromRoute] Guid EntryId)
        {
            return ((await _mapService.FindOneAsync(Id))?.Entries.SingleOrDefault(a => a.Id == EntryId))
                .Transient404(HttpContext);
        }

        /// <summary>
        ///     Delete all map entries for a specific map
        /// </summary>
        /// <response code="400">
        ///     * Unable to parse `Id`
        ///     * No map found for the given `Id`
        /// </response>
        [HttpDelete("{Id}/Entries")]
        [Authorize(Roles = "Admin,Developer")]
        [ProducesResponseType(204)]
        public async Task<ActionResult> DeleteMapEntriesAsync([FromRoute] Guid Id)
        {
            if (Id == Guid.Empty) return BadRequest("Error parsing Id");

            var map = await _mapService.FindOneAsync(Id);
            if (map == null) return BadRequest("No map with this id");

            map.Entries.Clear();
            await _mapService.ReplaceOneAsync(map);

            return NoContent();
        }

        /// <summary>
        ///     Delete a specific map entry for a specific map
        /// </summary>
        /// <response code="404">No entry found on the map for the given `EntryId`</response>
        /// <response code="400">
        ///     * Unable to parse `Id` or `EntryId`
        ///     * No map found for the given `Id`
        /// </response>
        [HttpDelete("{Id}/Entries/{EntryId}")]
        [Authorize(Roles = "Admin,Developer")]
        [ProducesResponseType(204)]
        public async Task<ActionResult> DeleteSingleMapEntryAsync([FromRoute] Guid Id, [FromRoute] Guid EntryId)
        {
            if (Id == Guid.Empty) return BadRequest("Error parsing Id");
            if (EntryId == Guid.Empty) return BadRequest("Error parsing EntryId");

            var map = await _mapService.FindOneAsync(Id);
            if (map == null) return BadRequest("No map with this id");

            var entry = map.Entries.SingleOrDefault(a => a.Id == EntryId);
            if (entry == null) return NotFound();

            map.Entries.Remove(entry);

            await _mapService.ReplaceOneAsync(map);

            return NoContent();
        }

        /// <summary>
        ///     Create a new map entry in a specific map
        /// </summary>
        /// <remarks>If you can generate guids client-side, you can also use the PUT variant for both create and update.</remarks>
        /// <param name="Record">Do not specify the "Id" property. It will be auto-assigned and returned in the response.</param>
        /// <returns>The id of the new map entry (guid)</returns>
        /// <response code="400">
        ///     * Unable to parse `Record` or `Id`
        /// </response>
        [HttpPost("{Id}/Entries")]
        [Authorize(Roles = "Admin,Developer")]
        [ProducesResponseType(typeof(Guid), 200)]
        public async Task<ActionResult> PostSingleMapEntryAsync([FromBody] MapEntryRecord Record, [FromRoute] Guid Id)
        {
            if (Record == null) return BadRequest("Error parsing Record");
            if (Id == Guid.Empty) return BadRequest("Error parsing Id");

            Record.Id = Guid.NewGuid();
            return await PutSingleMapEntryAsync(Record, Id, Record.Id);
        }

        /// <summary>
        ///     Create or Update an existing map entry in a specific map
        /// </summary>
        /// <remarks>
        ///     This both works for updating an existing entry and creating a new entry. The id property of the
        ///     model (request body) must match the {EntryId} part of the uri.
        /// </remarks>
        /// <param name="Record">"Id" property must match the {EntryId} part of the uri</param>
        /// <response code="400">
        ///     * Unable to parse `Record`, `Id` or `EntryId`
        ///     * `Record.Id` does not match `EntryId` from uri.
        ///     * No map found with for the specified id.
        /// </response>
        [HttpPut("{Id}/Entries/{EntryId}")]
        [Authorize(Roles = "Admin,Developer")]
        [ProducesResponseType(typeof(Guid), 200)]
        public async Task<ActionResult> PutSingleMapEntryAsync([FromBody] MapEntryRecord Record, [FromRoute] Guid Id,
            [FromRoute] Guid EntryId)
        {
            if (Record == null) return BadRequest("Error parsing Record");
            if (Id == Guid.Empty) return BadRequest("Error parsing Id");
            if (EntryId == Guid.Empty) return BadRequest("Error parsing EntryId");
            if (Record.Id != EntryId) return BadRequest("EntityId must match Record.Id");

            var map = await _mapService.FindOneAsync(Id);
            if (map == null) return BadRequest("No map with this id");

            var linkValidation = await _linkFragmentValidator.ValidateAsync(Record.Link);
            if (!linkValidation.IsValid) return BadRequest(linkValidation.ErrorMessage);

            map.Entries.Remove(map.Entries.SingleOrDefault(a => a.Id == EntryId));
            map.Entries.Add(Record);

            await _mapService.ReplaceOneAsync(map);
            return Ok(Record.Id);
        }
    }
}