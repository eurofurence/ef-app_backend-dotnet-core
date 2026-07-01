using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Domain.Model.Transformers;
using Eurofurence.App.Server.Services.Abstractions.Events;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/[controller]")]
    public class EventFeedbackController : BaseController
    {
        private readonly IEventFeedbackService _eventFeedbackService;
        private readonly IEventService _eventService;

        public EventFeedbackController(
            IEventFeedbackService eventFeedbackService,
            IEventService eventService)
        {
            _eventService = eventService;
            _eventFeedbackService = eventFeedbackService;
        }

        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [Authorize(Roles = "Attendee")]
        public async Task<ActionResult> PostEventFeedbackAsync([FromBody] PostEventFeedbackRequest request)
        {
            if (request == null) return BadRequest();

            if (await _eventService.FindOneAsync(request.EventId) == null)
                return BadRequest($"No event with id {request.EventId}");

            if (request.Rating < 1 || request.Rating > 5) return BadRequest("Rating must be between 1 and 5");

            var record = new EventFeedbackRecord()
            {
                EventId = request.EventId,
                Message = request.Message,
                Rating = request.Rating
            };

            record.Touch();
            record.NewId();

            await _eventFeedbackService.InsertOneAsync(record);

            return NoContent();
        }

        /// <summary>
        /// Returns all event feedback records filterable by event id.
        /// </summary>
        /// <param name="filterId">Optional event filter.</param>
        /// <returns>A list of all event's (or filtered) feedback.</returns>
        [Authorize(Roles = "EventFeedbackManager")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<EventFeedbackResponse>), 200)]
        public ActionResult GetEventFeedback(Guid filterId)
        {
            IEnumerable<EventFeedbackRecord> records = _eventFeedbackService.FindAll();
            if (filterId != Guid.Empty)
            {
                records = records.Where(x => x.EventId == filterId);
            }
            return Ok(records.Select(x => x.Transform()));
        }
    }
}