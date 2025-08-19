using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Knowledge;
using Eurofurence.App.Domain.Model.Transformers;
using Eurofurence.App.Server.Services.Abstractions.Knowledge;
using Eurofurence.App.Server.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

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
        [ProducesResponseType(typeof(IEnumerable<KnowledgeGroupResponse>), 200)]
        public IQueryable<KnowledgeGroupResponse> GetKnowledgeGroupsAsync()
        {
            return _knowledgeGroupService.FindAll().Select(x => x.Transform());
        }

        /// <summary>
        ///     Retrieve a single knowledge group.
        /// </summary>
        /// <param name="id">id of the requested entity</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(KnowledgeGroupResponse), 200)]
        public async Task<KnowledgeGroupResponse> GetKnowledgeGroupAsync(
            [EnsureNotNull][FromRoute] Guid id
            )
        {
            return (await _knowledgeGroupService.FindOneAsync(id)).Transient404(HttpContext)?.Transform();
        }

        /// <summary>
        ///     Update an existing knowledge group.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="id"></param>
        [Authorize(Roles = "Admin,KnowledgeBaseEditor")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 404)]
        [HttpPut("{id}")]
        public async Task<ActionResult> PutKnowledgeGroupAsync(
            [EnsureNotNull][FromBody] KnowledgeGroupRequest request,
            [EnsureNotNull][FromRoute] Guid id
        )
        {
            var existingRecord = await _knowledgeGroupService.FindOneAsync(id);
            if (existingRecord == null) return NotFound($"No record found with id {id}");

            existingRecord.Merge(request);
            existingRecord.Touch();
            await _knowledgeGroupService.ReplaceOneAsync(existingRecord);

            return NoContent();
        }

        /// <summary>
        ///     Create a new knowledge group.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>Id of the newly created knowledge group</returns>
        [Authorize(Roles = "Admin,KnowledgeBaseEditor")]
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(string), 409)]
        [HttpPost("")]
        public async Task<ActionResult> PostKnowledgeGroupAsync(
            [EnsureNotNull][FromBody] KnowledgeGroupRequest request
        )
        {
            KnowledgeGroupRecord record = request.Transform();
            record.Touch();
            await _knowledgeGroupService.InsertOneAsync(record);
            return Ok(record.Id);
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