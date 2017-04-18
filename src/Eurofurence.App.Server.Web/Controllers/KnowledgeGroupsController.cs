using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Eurofurence.App.Domain.Model.Sync;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Web.Extensions;
using Eurofurence.App.Domain.Model.Knowledge;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/[controller]")]
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

        /// <summary>
        /// Retrieves a delta of knowledge groups since a given timestamp.
        /// </summary>
        /// <param name="since" type="query">Delta reference, date time in ISO 8610. If set, only items with a 
        /// LastChangeDateTimeUtc >= the specified value will be returned. If not set, API will return the current set 
        /// of records without deleted items. If set, items deleted since the delta specified will be returned with an 
        /// IsDeleted flag set.</param>
        [HttpGet("Delta")]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(DeltaResponse<KnowledgeGroupRecord>), 200)]
        public Task<DeltaResponse<KnowledgeGroupRecord>> GetKnowledgeGroupsDeltaAsync([FromQuery] DateTime? since = null)
        {
            return _knowledgeGroupService.GetDeltaResponseAsync(minLastDateTimeChangedUtc: since);
        }
    }
}
