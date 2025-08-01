using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Domain.Model.Transformers;
using Eurofurence.App.Server.Services.Abstractions.Events;
using Eurofurence.App.Server.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        [ProducesResponseType(typeof(IEnumerable<EventConferenceRoomResponse>), 200)]
        public async Task<IEnumerable<EventConferenceRoomResponse>> GetEventsAsync()
        {
            var result = await _eventConferenceRoomService.FindAll().Select(x => x.Transform()).ToListAsync();
            result.ForEach(r => r.MapLink = _eventConferenceRoomService.GetMapLink(r.Id));
            return result;
        }

        /// <summary>
        ///     Retrieve a single event conference room in the event schedule.
        /// </summary>
        /// <param name="id">id of the requested entity</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(EventConferenceRoomResponse), 200)]
        public async Task<EventConferenceRoomResponse> GetEventAsync([FromRoute] Guid id)
        {
            var result = (await _eventConferenceRoomService.FindOneAsync(id)).Transient404(HttpContext).Transform();
            result.MapLink = _eventConferenceRoomService.GetMapLink(result.Id);
            return result;
        }
    }
}