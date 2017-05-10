using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Web.Extensions;
using Eurofurence.App.Domain.Model.Maps;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/v2/[controller]")]
    public class MapsController : Controller
    {
        readonly IMapService _mapService;
        readonly ILinkFragmentValidator _linkFragmentValidator;

        public MapsController(IMapService mapService, ILinkFragmentValidator linkFragmentValidator)
        {
            _mapService = mapService;
            _linkFragmentValidator = linkFragmentValidator;
        }

        /// <summary>
        /// Get all maps
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(IEnumerable<MapRecord>), 200)]
        public Task<IEnumerable<MapRecord>> GetMapsAsync()
        {
            return _mapService.FindAllAsync();
        }

        /// <summary>
        /// Get a specific map
        /// </summary>
        [HttpGet("{Id}")]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(MapRecord), 200)]
        public async Task<MapRecord> GetMapAsync([FromRoute] Guid id)
        {
            return (await _mapService.FindOneAsync(id)).Transient404(HttpContext);
        }


        /// <summary>
        /// Get all map entries for a specific map
        /// </summary>
        [HttpGet("{Id}/Entries")]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(ICollection<MapEntryRecord>), 200)]
        public async Task<ICollection<MapEntryRecord>> GetMapEntriesAsync([FromRoute] Guid id)
        {
            return (await _mapService.FindOneAsync(id)).Transient404(HttpContext)?.Entries;
        }

        /// <summary>
        /// Get all specific map entry for a specific map
        /// </summary>
        [HttpGet("{Id}/Entries/{EntryId}")]
        [ProducesResponseType(404)]
        [ProducesResponseType(typeof(MapEntryRecord), 200)]
        public async Task<MapEntryRecord> GetSingleMapEntryAsync([FromRoute] Guid id, [FromRoute] Guid entryId)
        {
            return ((await _mapService.FindOneAsync(id))?.
                Entries.SingleOrDefault(a => a.Id == entryId))
                .Transient404(HttpContext);
        }

        /// <summary>
        /// Delete all map entries for a specific map
        /// </summary>
        [HttpDelete("{Id}/Entries")]
        [Authorize(Roles = "Admin,Developer")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult> DeleteMapEntriesAsync([FromRoute] Guid id)
        {
            var map = await _mapService.FindOneAsync(id);
            if (map == null) return BadRequest("No map with this id");

            map.Entries.Clear();
            await _mapService.ReplaceOneAsync(map);

            return Ok();
        }
        /// <summary>
        /// Delete a specific map entry for a specific map
        /// </summary>
        [HttpDelete("{Id}/Entries/{EntryId}")]
        [Authorize(Roles = "Admin,Developer")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> DeleteSingleMapEntryAsync([FromRoute] Guid id, [FromRoute] Guid entryId)
        {
            var map = await _mapService.FindOneAsync(id);
            if (map == null) return BadRequest("No map with this id");

            var entry = map.Entries.SingleOrDefault(a => a.Id == entryId);
            if (entry == null) return NotFound();

            map.Entries.Remove(entry);

            await _mapService.ReplaceOneAsync(map);

            return Ok();
        }

        /// <summary>
        /// Create a new map entry in a specific map
        /// </summary>
        /// <remarks>If you can generate guids client-side, you can also use the PUT variant for both create and update.</remarks>
        /// <param name="record">Do not specify the "Id" property. It will be auto-assigned and returned in the response.</param>
        /// <returns>The id of the new map entry (guid)</returns>
        [HttpPost("{Id}/Entries")]
        [Authorize(Roles = "Admin,Developer")]
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult> PostSingleMapEntryAsync([FromBody] MapEntryRecord record, [FromRoute] Guid id)
        {
            if (record == null) return BadRequest();

            record.Id = Guid.NewGuid();
            return await PutSingleMapEntryAsync(record, id, record.Id);
        }

        /// <summary>
        /// Create or Update an existing map entry in a specific map
        /// </summary>
        /// <remarks>This both works for updating an existing entry and creating a new entry. The id property of the
        /// model (request body) must match the {EntryId} part of the uri.
        /// </remarks>
        /// <param name="record">"Id" property must match the {EntryId} part of the uri</param>
        [HttpPut("{Id}/Entries/{EntryId}")]
        [Authorize(Roles = "Admin,Developer")]
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult> PutSingleMapEntryAsync([FromBody] MapEntryRecord record, [FromRoute] Guid id, [FromRoute] Guid entryId)
        {
            if (record == null) return BadRequest();
            if (record.Id != entryId) return BadRequest("Entity id must match resource id");

            var map = await _mapService.FindOneAsync(id);
            if (map == null) return BadRequest("No map with this id");

            var linkValidation = await _linkFragmentValidator.ValidateAsync(record.Link);
            if (!linkValidation.IsValid) return BadRequest(linkValidation.ErrorMessage);

            map.Entries.Remove(map.Entries.SingleOrDefault(a => a.Id == entryId));
            map.Entries.Add(record);

            await _mapService.ReplaceOneAsync(map);
            return Ok(record.Id);
        }
    }
}
