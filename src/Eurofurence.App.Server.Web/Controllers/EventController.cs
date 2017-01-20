using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Eurofurence.App.Domain.Model;
using Eurofurence.App.Server.Services.Abstractions;

namespace Eurofurence.App.Server.Web.Controllers
{

    [Route("Api/[controller]")]
    public class EventController : Controller
    {
        private readonly IEventService _eventService;

        public EventController(IEventService eventService)
        {
            _eventService = eventService;
        }

        /// <summary>
        /// Retrieves a list of all events in the event schedule.
        /// </summary>
        /// <param name="since">Delta reference, date time in ISO 8610. If set, only items with a 
        /// LastChangeDateTimeUtc >= the specified value will be returned. If not set, API will return the current set 
        /// of records without deleted items. If set, items deleted since the delta specified will be returned with an 
        /// IsDeleted flag set.</param>
        /// <returns>All events in the event schedule.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(EventRecord[]), 200)]
        public Task<IEnumerable<EventRecord>> GetEventsAsync(DateTime? since = null)
        {
            return _eventService.FindAllAsync(minLastDateTimeChangedUtc: since);
        }
    }
}
