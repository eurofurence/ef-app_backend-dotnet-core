using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Domain.Model.Transformers;
using Eurofurence.App.Server.Services.Abstractions.Events;
using Eurofurence.App.Server.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/[controller]")]
    public class EventConferenceDaysController : BaseController
    {
        private readonly IEventConferenceDayService _eventConferenceDayService;

        public EventConferenceDaysController(IEventConferenceDayService eventConferenceDayService)
        {
            _eventConferenceDayService = eventConferenceDayService;
        }

        /// <summary>
        ///     Retrieves a list of all event conference days in the event schedule.
        /// </summary>
        /// <returns>All events in the event schedule.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(IEnumerable<EventConferenceDayResponse>), 200)]
        public IQueryable<EventConferenceDayResponse> GetEventsAsync()
        {
            return _eventConferenceDayService.FindAll().Select(x => x.Transform());
        }

        /// <summary>
        ///     Retrieve a single event conference day in the event schedule.
        /// </summary>
        /// <param name="id">id of the requested entity</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(EventConferenceDayResponse), 200)]
        public async Task<EventConferenceDayResponse> GetEventAsync([FromRoute] Guid id)
        {
            return (await _eventConferenceDayService.FindOneAsync(id)).Transient404(HttpContext)?.Transform();
        }
    }
}