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
        ///     Update an existing knowledge entry.
        /// </summary>
        /// <param name="Record"></param>
        /// <param name="Id"></param>
        [Authorize(Roles = "System,Developer,KnowledgeBase-Maintainer")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 404)]
        [EnsureNotNull][HttpPut("{Id}")]
        public async Task<ActionResult> PutKnowledgeEntryAsync(
            [EnsureNotNull][FromBody][EnsureEntityIdMatches("Id")] KnowledgeEntryRecord Record,
            [EnsureNotNull][FromRoute] Guid Id)
        {
            var existingRecord = await _knowledgeEntryService.FindOneAsync(Id);
            if (existingRecord == null) return NotFound($"No record found with it {Id}");

            Record.Touch();
            await _knowledgeEntryService.ReplaceOneAsync(Record);

            return NoContent();
        }

        /// <summary>
        ///     Create a new knowledge entry.
        /// </summary>
        /// <param name="Record"></param>
        /// <returns>Id of the newly created knowledge entry</returns>
        [Authorize(Roles = "System,Developer,KnowledgeBase-Maintainer")]
        [ProducesResponseType(typeof(Guid), 200)]
        [HttpPost("")]
        public async Task<ActionResult> PostKnowledgeEntryAsync(
            [EnsureNotNull][FromBody] KnowledgeEntryRecord Record
        )
        {
            Record.NewId();
            Record.Touch();
            await _knowledgeEntryService.InsertOneAsync(Record);

            return Ok(Record.Id);
        }

        /// <summary>
        ///     Delete a knowledge entry.
        /// </summary>
        /// <param name="Id"></param>
        [Authorize(Roles = "System,Developer,KnowledgeBase-Maintainer")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 404)]
        [HttpDelete("{Id}")]
        public async Task<ActionResult> DeleteKnowledgeEntryAsync(
            [EnsureNotNull][FromRoute] Guid Id
        )
        {
            var existingRecord = await _knowledgeEntryService.FindOneAsync(Id);
            if (existingRecord == null) return NotFound($"No record found with it {Id}");

            await _knowledgeEntryService.DeleteOneAsync(Id);

            return NoContent();
        }

    }
}