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
    public class EventConferenceTracksController : BaseController
    {
        private readonly IEventConferenceTrackService _eventConferenceTrackService;

        public EventConferenceTracksController(IEventConferenceTrackService eventConferenceTrackService)
        {
            _eventConferenceTrackService = eventConferenceTrackService;
        }

        /// <summary>
        ///     Retrieves a list of all event conference tracks in the event schedule.
        /// </summary>
        /// <returns>All events in the event schedule.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(IEnumerable<EventConferenceTrackRecord>), 200)]
        public IQueryable<EventConferenceTrackRecord> GetEventsAsync()
        {
            return _eventConferenceTrackService.FindAll();
        }

        /// <summary>
        ///     Retrieve a single event conference track in the event schedule.
        /// </summary>
        /// <param name="id">id of the requested entity</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(EventConferenceTrackRecord), 200)]
        public async Task<EventConferenceTrackRecord> GetEventAsync([FromRoute] Guid id)
        {
            return (await _eventConferenceTrackService.FindOneAsync(id)).Transient404(HttpContext);
        }
    }
}