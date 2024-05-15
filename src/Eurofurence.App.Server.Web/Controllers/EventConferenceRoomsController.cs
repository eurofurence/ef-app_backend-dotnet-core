using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Server.Services.Abstractions.Events;
using Eurofurence.App.Server.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/[controller]")]
    public class EventConferenceRoomsController : BaseController
    {
        private readonly IEventConferenceRoomService _eventConferenceRoomService;

        public EventConferenceRoomsController(IEventConferenceRoomService eventConferenceRoomService)
        {
            _eventConferenceRoomService = eventConferenceRoomService;
        }

        /// <summary>
        ///     Retrieves a list of all event conference Rooms in the event schedule.
        /// </summary>
        /// <returns>All events in the event schedule.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(IEnumerable<EventConferenceRoomRecord>), 200)]
        public IQueryable<EventConferenceRoomRecord> GetEventsAsync()
        {
            return _eventConferenceRoomService.FindAll();
        }

        /// <summary>
        ///     Retrieve a single event conference room in the event schedule.
        /// </summary>
        /// <param name="id">id of the requested entity</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(EventConferenceRoomRecord), 200)]
        public async Task<EventConferenceRoomRecord> GetEventAsync([FromRoute] Guid id)
        {
            return (await _eventConferenceRoomService.FindOneAsync(id)).Transient404(HttpContext);
        }
    }
}