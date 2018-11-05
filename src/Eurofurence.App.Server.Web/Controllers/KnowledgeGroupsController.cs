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
    [Route("Api/[controller]")]
    public class KnowledgeGroupsController : BaseController
    {
        private readonly IKnowledgeGroupService _knowledgeGroupService;

        public KnowledgeGroupsController(IKnowledgeGroupService knowledgeGroupService)
        {
            _knowledgeGroupService = knowledgeGroupService;
        }

        /// <summary>
        ///     Retrieves a list of all knowledge groups.
        /// </summary>
        /// <returns>All knowledge groups.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(IEnumerable<KnowledgeGroupRecord>), 200)]
        public Task<IEnumerable<KnowledgeGroupRecord>> GetKnowledgeGroupsAsync()
        {
            return _knowledgeGroupService.FindAllAsync();
        }

        /// <summary>
        ///     Retrieve a single knowledge group.
        /// </summary>
        /// <param name="Id">id of the requested entity</param>
        [HttpGet("{Id}")]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(KnowledgeGroupRecord), 200)]
        public async Task<KnowledgeGroupRecord> GetKnowledgeGroupAsync(
            [EnsureNotNull][FromRoute] Guid Id
            )
        {
            return (await _knowledgeGroupService.FindOneAsync(Id)).Transient404(HttpContext);
        }

        /// <summary>
        ///     Update an existing knowledge group.
        /// </summary>
        /// <param name="Record"></param>
        /// <param name="Id"></param>
        [Authorize(Roles = "System,Developer,KnowledgeBase-Maintainer")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 404)]
        [HttpPut("{Id}")]
        public async Task<ActionResult> PutKnowledgeGroupAsync(
            [EnsureNotNull][FromBody][EnsureEntityIdMatches("Id")] KnowledgeGroupRecord Record,
            [EnsureNotNull][FromRoute] Guid Id
        )
        {
            var existingRecord = await _knowledgeGroupService.FindOneAsync(Id);
            if (existingRecord == null) return NotFound($"No record found with it {Id}");

            Record.Touch();
            await _knowledgeGroupService.ReplaceOneAsync(Record);

            return NoContent();
        }

        /// <summary>
        ///     Create a new knowledge group.
        /// </summary>
        /// <param name="Record"></param>
        /// <returns>Id of the newly created knowledge group</returns>
        [Authorize(Roles = "System,Developer,KnowledgeBase-Maintainer")]
        [ProducesResponseType(typeof(Guid), 200)]
        [HttpPost("")]
        public async Task<ActionResult> PostKnowledgeGroupAsync(
            [EnsureNotNull][FromBody] KnowledgeGroupRecord Record
        )
        {
            Record.NewId();
            Record.Touch();
            await _knowledgeGroupService.InsertOneAsync(Record);

            return Ok(Record.Id);
        }

        /// <summary>
        ///     Delete a knowledge group.
        /// </summary>
        /// <param name="Id"></param>
        [Authorize(Roles = "System,Developer,KnowledgeBase-Maintainer")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 404)]
        [HttpDelete("{Id}")]
        public async Task<ActionResult> DeleteKnowledgeGroupAsync(
            [EnsureNotNull][FromRoute] Guid Id
        )
        {
            var existingRecord = await _knowledgeGroupService.FindOneAsync(Id);
            if (existingRecord == null) return NotFound($"No record found with it {Id}");

            await _knowledgeGroupService.DeleteOneAsync(Id);

            return NoContent();
        }

    }
}