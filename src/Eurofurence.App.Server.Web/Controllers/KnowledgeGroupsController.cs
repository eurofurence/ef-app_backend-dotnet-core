using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Knowledge;
using Eurofurence.App.Server.Services.Abstractions.Knowledge;
using Eurofurence.App.Server.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Eurofurence.App.Server.Services.Abstractions;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/[controller]")]
    public class KnowledgeGroupsController : BaseController
    {
        private readonly IKnowledgeGroupService _knowledgeGroupService;
        private readonly ILogger _logger;

        public KnowledgeGroupsController(IKnowledgeGroupService knowledgeGroupService,
            ILoggerFactory loggerFactory)
        {
            _knowledgeGroupService = knowledgeGroupService;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        /// <summary>
        ///     Retrieves a list of all knowledge groups.
        /// </summary>
        /// <returns>All knowledge groups.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(IEnumerable<KnowledgeGroupRecord>), 200)]
        public IQueryable<KnowledgeGroupRecord> GetKnowledgeGroupsAsync()
        {
            return _knowledgeGroupService.FindAll();
        }

        /// <summary>
        ///     Retrieve a single knowledge group.
        /// </summary>
        /// <param name="id">id of the requested entity</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(KnowledgeGroupRecord), 200)]
        public async Task<KnowledgeGroupRecord> GetKnowledgeGroupAsync(
            [EnsureNotNull][FromRoute] Guid id
            )
        {
            return (await _knowledgeGroupService.FindOneAsync(id)).Transient404(HttpContext);
        }

        /// <summary>
        ///     Update an existing knowledge group.
        /// </summary>
        /// <param name="record"></param>
        /// <param name="id"></param>
        [Authorize(Roles = "Admin,KnowledgeBaseEditor")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 404)]
        [HttpPut("{id}")]
        public async Task<ActionResult> PutKnowledgeGroupAsync(
            [EnsureNotNull][FromBody][EnsureEntityIdMatches("id")] KnowledgeGroupRecord record,
            [EnsureNotNull][FromRoute] Guid id
        )
        {
            var existingRecord = await _knowledgeGroupService.FindOneAsync(id);
            if (existingRecord == null) return NotFound($"No record found with id {id}");

            record.Touch();
            await _knowledgeGroupService.ReplaceOneAsync(record);

            return NoContent();
        }

        /// <summary>
        ///     Create a new knowledge group.
        /// </summary>
        /// <param name="Record"></param>
        /// <returns>Id of the newly created knowledge group</returns>
        [Authorize(Roles = "Admin,KnowledgeBaseEditor")]
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(string), 409)]
        [HttpPost("")]
        public async Task<ActionResult> PostKnowledgeGroupAsync(
            [EnsureNotNull][FromBody] KnowledgeGroupRecord Record
        )
        {
            try
            {
                Record.Touch();
                await _knowledgeGroupService.InsertOneAsync(Record);
                return Ok(Record.Id);
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
            {
                if ((e.InnerException as MySqlConnector.MySqlException)?.ErrorCode == MySqlConnector.MySqlErrorCode.DuplicateKeyEntry)
                {
                    return Conflict($"Record with id {Record.Id} already exists.");
                }
                return BadRequest();
            }
        }

        /// <summary>
        ///     Delete a knowledge group.
        /// </summary>
        /// <param name="id"></param>
        [Authorize(Roles = "Admin,KnowledgeBaseEditor")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 404)]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteKnowledgeGroupAsync(
            [EnsureNotNull][FromRoute] Guid id
        )
        {
            var existingRecord = await _knowledgeGroupService.FindOneAsync(id);
            if (existingRecord == null) return NotFound($"No record found with id {id}");

            await _knowledgeGroupService.DeleteOneAsync(id);

            return NoContent();
        }

    }
}