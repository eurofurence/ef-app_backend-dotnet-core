using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Knowledge;
using Eurofurence.App.Server.Services.Abstractions.Knowledge;
using Eurofurence.App.Server.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/[controller]")]
    public class KnowledgeEntriesController : BaseController
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
        [Authorize]
        public IQueryable<KnowledgeEntryRecord> GetKnowledgeEntriesAsync()
        {
            return _knowledgeEntryService.FindAll()
                .Include(ke => ke.Images)
                .Include(ke => ke.Links);
        }

        /// <summary>
        ///     Retrieve a single knowledge entry.
        /// </summary>
        /// <param name="id">id of the requested entity</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(KnowledgeEntryRecord), 200)]
        public async Task<KnowledgeEntryRecord> GetKnowledgeEntryAsync([FromRoute] Guid id)
        {
            return (await _knowledgeEntryService.FindAll()
                .Include(ke => ke.Images)
                .Include(ke => ke.Links)
                .FirstOrDefaultAsync(entity => entity.Id == id)).Transient404(HttpContext);
        }


        /// <summary>
        ///     Update an existing knowledge entry.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="id"></param>
        [Authorize(Roles = "System,Developer,KnowledgeBase-Maintainer")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 404)]
        [EnsureNotNull][HttpPut("{id}")]
        public async Task<ActionResult> PutKnowledgeEntryAsync(
            [EnsureNotNull][FromBody] KnowledgeEntryRequest request,
            [EnsureNotNull][FromRoute] Guid id)
        {
            var existingRecord = await _knowledgeEntryService.FindOneAsync(id);
            if (existingRecord == null) return NotFound($"No record found with it {id}");

            await _knowledgeEntryService.ReplaceKnowledgeEntryAsync(id, request);

            return NoContent();
        }

        /// <summary>
        ///     Create a new knowledge entry.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Id of the newly created knowledge entry</returns>
        [Authorize(Roles = "System,Developer,KnowledgeBase-Maintainer")]
        [ProducesResponseType(typeof(Guid), 200)]
        [HttpPost("")]
        public async Task<ActionResult> PostKnowledgeEntryAsync(
            [EnsureNotNull][FromBody] KnowledgeEntryRequest request
        )
        {
            var result = await _knowledgeEntryService.InsertKnowledgeEntryAsync(request);

            return Ok(result);
        }

        /// <summary>
        ///     Delete a knowledge entry.
        /// </summary>
        /// <param name="id"></param>
        [Authorize(Roles = "System,Developer,KnowledgeBase-Maintainer")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 404)]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteKnowledgeEntryAsync(
            [EnsureNotNull][FromRoute] Guid id
        )
        {
            var existingRecord = await _knowledgeEntryService.FindOneAsync(id);
            if (existingRecord == null) return NotFound($"No record found with it {id}");

            await _knowledgeEntryService.DeleteOneAsync(id);

            return NoContent();
        }

    }
}