using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Eurofurence.App.Domain.Model.Sync;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Web.Extensions;
using Eurofurence.App.Domain.Model.Dealers;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/[controller]")]
    public class DealersController : Controller
    {
        readonly IDealerService _dealerService;

        public DealersController(IDealerService dealerService)
        {
            _dealerService = dealerService;
        }

        /// <summary>
        /// Retrieves a list of all dealer entries.
        /// </summary>
        /// <returns>All dealer Entries.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(IEnumerable<DealerRecord>), 200)]
        public Task<IEnumerable<DealerRecord>> GetDealerEntriesAsync()
        {
            return _dealerService.FindAllAsync();
        }

        /// <summary>
        /// Retrieve a single dealer.
        /// </summary>
        /// <param name="id">id of the requested entity</param>
        [HttpGet("{Id}")]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(DealerRecord), 200)]
        public async Task<DealerRecord> GetDealerAsync([FromRoute] Guid id)
        {
            return (await _dealerService.FindOneAsync(id)).Transient404(HttpContext);
        }

        /// <summary>
        /// Retrieves a delta of dealer entries since a given timestamp.
        /// </summary>
        /// <param name="since" type="query">Delta reference, date time in ISO 8610. If set, only items with a 
        /// LastChangeDateTimeUtc >= the specified value will be returned. If not set, API will return the current set 
        /// of records without deleted items. If set, items deleted since the delta specified will be returned with an 
        /// IsDeleted flag set.</param>
        [HttpGet("Delta")]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(DeltaResponse<DealerRecord>), 200)]
        public Task<DeltaResponse<DealerRecord>> GetDealerEntriesDeltaAsync([FromQuery] DateTime? since = null)
        {
            return _dealerService.GetDeltaResponseAsync(minLastDateTimeChangedUtc: since);
        }
    }
}
