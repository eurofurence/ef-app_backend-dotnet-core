using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Eurofurence.App.Server.Web.Extensions;
using Eurofurence.App.Domain.Model.Knowledge;
using Eurofurence.App.Server.Services.Abstractions.Knowledge;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/v2/[controller]")]
    public class KnowledgeGroupsController : Controller
    {
        readonly IKnowledgeGroupService _knowledgeGroupService;

        public KnowledgeGroupsController(IKnowledgeGroupService knowledgeGroupService)
        {
            _knowledgeGroupService = knowledgeGroupService;
        }

        /// <summary>
        /// Retrieves a list of all knowledge groups.
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
        /// Retrieve a single knowledge group.
        /// </summary>
        /// <param name="id">id of the requested entity</param>
        [HttpGet("{Id}")]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(KnowledgeGroupRecord), 200)]
        public async Task<KnowledgeGroupRecord> GetKnowledgeGroupAsync([FromRoute] Guid id)
        {
            return (await _knowledgeGroupService.FindOneAsync(id)).Transient404(HttpContext);
        }
    }
}
