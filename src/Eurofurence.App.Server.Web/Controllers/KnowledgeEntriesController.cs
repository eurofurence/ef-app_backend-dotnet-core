using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Knowledge;
using Eurofurence.App.Server.Services.Abstractions.Knowledge;
using Eurofurence.App.Server.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/v2/[controller]")]
    public class KnowledgeEntriesController : Controller
    {
        private readonly IKnowledgeEntryService _knowledgeEntryService;

        public KnowledgeEntriesController(IKnowledgeEntryService knowledgeEntryService)
        {
            _knowledgeEntryService = knowledgeEntryService;
        }

        /// <summary>
        ///     Retrieves a list of all knowledge entries.
        /// </summary>
        /// <returns>All knowledge Entries.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(IEnumerable<KnowledgeEntryRecord>), 200)]
        public Task<IEnumerable<KnowledgeEntryRecord>> GetKnowledgeEntriesAsync()
        {
            return _knowledgeEntryService.FindAllAsync();
        }

        /// <summary>
        ///     Retrieve a single knowledge entry.
        /// </summary>
        /// <param name="id">id of the requested entity</param>
        [HttpGet("{Id}")]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(KnowledgeEntryRecord), 200)]
        public async Task<KnowledgeEntryRecord> GetKnowledgeEntryAsync([FromRoute] Guid id)
        {
            return (await _knowledgeEntryService.FindOneAsync(id)).Transient404(HttpContext);
        }


        /// <summary>
        ///     Create a new knowledge entry
        /// </summary>
        /// <param name="Record"></param>
        /// <param name="Id"></param>
        [Authorize(Roles = "System,Developer")]
        [ProducesResponseType(204)]
        [HttpPut("{Id}")]
        public async Task<ActionResult> PutKnowledgeEntryAsync([FromBody] KnowledgeEntryRecord Record, [FromRoute] Guid Id)
        {
            if (Record == null) return BadRequest("Error validating Record");
            if (Id == Guid.Empty || Id != Record.Id) return BadRequest("Error validating Id");

            var existingRecord = await _knowledgeEntryService.FindOneAsync(Record.Id);
            if (existingRecord == null) return NotFound($"No record found with it {Record.Id}");

            Record.Touch();
            await _knowledgeEntryService.ReplaceOneAsync(Record);

            return NoContent();
        }

        /// <summary>
        ///     Update an existing knowledge entry
        /// </summary>
        /// <param name="record"></param>
        /// <returns>Id of the newly created knowledge entry</returns>
        [Authorize(Roles = "System,Developer")]
        [ProducesResponseType(typeof(Guid), 200)]
        [HttpPost("")]
        public async Task<ActionResult> PostKnowledgeEntryAsync([FromBody] KnowledgeEntryRecord record)
        {
            record.NewId();
            record.Touch();
            await _knowledgeEntryService.InsertOneAsync(record);

            return Ok(record.Id);
        }

        [Authorize(Roles = "System,Developer")]
        [ProducesResponseType(204)]
        [HttpDelete("{Id}")]
        [HttpDelete("")]
        public async Task<ActionResult> DeleteKnowledgeEntryAsync([FromRoute] Guid Id)
        {
            if (Id == Guid.Empty) return BadRequest("Error validating Id");

            var existingRecord = await _knowledgeEntryService.FindOneAsync(Id);
            if (existingRecord == null) return NotFound($"No record found with it {Id}");

            await _knowledgeEntryService.DeleteOneAsync(Id);

            return NoContent();
        }

    }
}