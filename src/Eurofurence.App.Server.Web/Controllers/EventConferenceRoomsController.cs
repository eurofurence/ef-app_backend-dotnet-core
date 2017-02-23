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
    [Route("Api/[controller]")]
    public class EventConferenceRoomsController : Controller
    {
        readonly IEventConferenceRoomService _eventConferenceRoomService;

        public EventConferenceRoomsController(IEventConferenceRoomService eventConferenceRoomService)
        {
            _eventConferenceRoomService = eventConferenceRoomService;
        }

        /// <summary>
        /// Retrieves a list of all event conference Rooms in the event schedule.
        /// </summary>
        /// <returns>All events in the event schedule.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(IEnumerable<EventConferenceRoomRecord>), 200)]
        public Task<IEnumerable<EventConferenceRoomRecord>> GetEventsAsync()
        {
            return _eventConferenceRoomService.FindAllAsync();
        }
        
        /// <summary>
        /// Retrieve a single event conference room in the event schedule.
        /// </summary>
        /// <param name="id">id of the requested entity</param>
        [HttpGet("{Id}")]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(EventConferenceRoomRecord), 200)]
        public async Task<EventConferenceRoomRecord> GetEventAsync([FromRoute] Guid id)
        {
            return (await _eventConferenceRoomService.FindOneAsync(id)).Transient404(HttpContext);
        }

        /// <summary>
        /// Retrieves a delta of event conference rooms in the event schedule since a given timestamp.
        /// </summary>
        /// <param name="since" type="query">Delta reference, date time in ISO 8610. If set, only items with a 
        /// LastChangeDateTimeUtc >= the specified value will be returned. If not set, API will return the current set 
        /// of records without deleted items. If set, items deleted since the delta specified will be returned with an 
        /// IsDeleted flag set.</param>
        [HttpGet("Delta")]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(DeltaResponse<EventConferenceRoomRecord>), 200)]
        public Task<DeltaResponse<EventConferenceRoomRecord>> GetEventsDeltaAsync([FromQuery] DateTime? since = null)
        {
            return _eventConferenceRoomService.GetDeltaResponseAsync(minLastDateTimeChangedUtc: since);
        }
    }
}
