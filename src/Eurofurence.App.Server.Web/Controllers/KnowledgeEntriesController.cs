using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Knowledge;
using Eurofurence.App.Server.Services.Abstractions.Knowledge;
using Eurofurence.App.Server.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/v2/[controller]")]
    public class KnowledgeEntriesController : Controller
    {
        private readonly IKnowledgeEntryService _knowledgeEntriyService;

        public KnowledgeEntriesController(IKnowledgeEntryService knowledgeEntryService)
        {
            _knowledgeEntriyService = knowledgeEntryService;
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
            return _knowledgeEntriyService.FindAllAsync();
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
            return (await _knowledgeEntriyService.FindOneAsync(id)).Transient404(HttpContext);
        }
    }
}