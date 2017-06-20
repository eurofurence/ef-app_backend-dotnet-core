using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Domain.Model.Sync;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Events;
using Eurofurence.App.Server.Web.Extensions;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/v2/[controller]")]
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
    }
}
