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
using Eurofurence.App.Domain.Model.Transformers;

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
        [ProducesResponseType(typeof(IEnumerable<KnowledgeEntryResponse>), 200)]
        public IEnumerable<KnowledgeEntryResponse> GetKnowledgeEntriesAsync(bool includeNonPublished = false)
        {
            var knowledgeEntries = _knowledgeEntryService.FindAll();

            if (!includeNonPublished || !(User.IsInRole("KnowledgeBaseEditor") || User.IsInRole("Admin")))
            {
                knowledgeEntries = knowledgeEntries.Where(ke => ke.Published != null);
            }

            return knowledgeEntries.Select(x => x.Transform());
        }

        /// <summary>
        ///     Retrieve a single knowledge entry.
        /// </summary>
        /// <param name="id">id of the requested entity</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(KnowledgeEntryResponse), 200)]
        public async Task<KnowledgeEntryResponse> GetKnowledgeEntryAsync([FromRoute] Guid id)
        {
            var knowledgeEntryResponse = (await _knowledgeEntryService.FindOneAsync(id)).Transform();

            if (knowledgeEntryResponse?.Published == null && !User.IsInRole("KnowledgeBaseEditor") && !User.IsInRole("Admin"))
            {
                knowledgeEntryResponse = null;
            }

            return knowledgeEntryResponse.Transient404(HttpContext);
        }


        /// <summary>
        ///     Update an existing knowledge entry.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="id"></param>
        [Authorize(Roles = "Admin,KnowledgeBaseEditor")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 404)]
        [EnsureNotNull]
        [HttpPut("{id}")]
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
        [Authorize(Roles = "Admin,KnowledgeBaseEditor")]
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(string), 409)]
        [HttpPost("")]
        public async Task<ActionResult> PostKnowledgeEntryAsync(
            [EnsureNotNull][FromBody] KnowledgeEntryRequest request
        )
        {
            try
            {
                var result = await _knowledgeEntryService.InsertKnowledgeEntryAsync(request);
                return Ok(result.Id);
            }
            catch (DbUpdateException e)
            {
                if ((e.InnerException as MySqlConnector.MySqlException)?.ErrorCode == MySqlConnector.MySqlErrorCode.DuplicateKeyEntry)
                {
                    return Conflict($"Record with id {request.Id} already exists.");
                }
                return BadRequest();
            }
        }

        /// <summary>
        ///     Delete a knowledge entry.
        /// </summary>
        /// <param name="id"></param>
        [Authorize(Roles = "Admin,KnowledgeBaseEditor")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 404)]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteKnowledgeEntryAsync(
            [EnsureNotNull][FromRoute] Guid id
        )
        {
            var existingRecord = await _knowledgeEntryService.FindOneAsync(id);
            if (existingRecord == null) return NotFound($"No record found with id {id}");

            await _knowledgeEntryService.DeleteOneAsync(id);

            return NoContent();
        }

    }
}