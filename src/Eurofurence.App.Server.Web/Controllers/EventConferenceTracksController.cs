using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Domain.Model.Sync;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Web.Extensions;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/v2/[controller]")]
    public class EventConferenceTracksController : Controller
    {
        readonly IEventConferenceTrackService _eventConferenceTrackService;

        public EventConferenceTracksController(IEventConferenceTrackService eventConferenceTrackService)
        {
            _eventConferenceTrackService = eventConferenceTrackService;
        }

        /// <summary>
        /// Retrieves a list of all event conference tracks in the event schedule.
        /// </summary>
        /// <returns>All events in the event schedule.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(IEnumerable<EventConferenceTrackRecord>), 200)]
        public Task<IEnumerable<EventConferenceTrackRecord>> GetEventsAsync()
        {
            return _eventConferenceTrackService.FindAllAsync();
        }
        
        /// <summary>
        /// Retrieve a single event conference track in the event schedule.
        /// </summary>
        /// <param name="id">id of the requested entity</param>
        [HttpGet("{Id}")]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(EventConferenceTrackRecord), 200)]
        public async Task<EventConferenceTrackRecord> GetEventAsync([FromRoute] Guid id)
        {
            return (await _eventConferenceTrackService.FindOneAsync(id)).Transient404(HttpContext);
        }

        /// <summary>
        /// Retrieves a delta of event conference tracks in the event schedule since a given timestamp.
        /// </summary>
        /// <param name="since" type="query">Delta reference, date time in ISO 8610. If set, only items with a 
        /// LastChangeDateTimeUtc >= the specified value will be returned. If not set, API will return the current set 
        /// of records without deleted items. If set, items deleted since the delta specified will be returned with an 
        /// IsDeleted flag set.</param>
        [HttpGet("Delta")]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(DeltaResponse<EventConferenceTrackRecord>), 200)]
        public Task<DeltaResponse<EventConferenceTrackRecord>> GetEventsDeltaAsync([FromQuery] DateTime? since = null)
        {
            return _eventConferenceTrackService.GetDeltaResponseAsync(minLastDateTimeChangedUtc: since);
        }
    }
}
