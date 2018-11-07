using System.Collections.Generic;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Server.Services.Abstractions.Events;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/[controller]")]
    public class EventFeedbackController : BaseController
    {
        private readonly IApiPrincipal _apiPrincipal;
        private readonly IEventFeedbackService _eventFeedbackService;
        private readonly IEventService _eventService;

        public EventFeedbackController(
            IEventFeedbackService eventFeedbackService,
            IEventService eventService,
            IApiPrincipal apiPrincipal)
        {
            _eventService = eventService;
            _apiPrincipal = apiPrincipal;
            _eventFeedbackService = eventFeedbackService;
        }

        [HttpGet]
        [Authorize(Roles = "System,Developer,Attendee")]
        [ProducesResponseType(typeof(IEnumerable<EventFeedbackRecord>), 200)]
        public Task<IEnumerable<EventFeedbackRecord>> GetEventFeedbackAsync()
        {
            if (_apiPrincipal.IsAttendee)
                return _eventFeedbackService.FindAllAsync(a => a.AuthorUid == _apiPrincipal.Uid);

            return null;
        }

        [HttpPost]
        [Authorize(Roles = "System,Developer,Attendee")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> PostEventFeedbackAsync([FromBody] EventFeedbackRecord record)
        {
            if (record == null) return BadRequest();

            if (await _eventService.FindOneAsync(record.EventId) == null)
                return BadRequest($"No event with id {record.EventId}");

            record.Touch();
            record.NewId();
            record.AuthorUid = _apiPrincipal.Uid;

            await _eventFeedbackService.InsertOneAsync(record);

            return NoContent();
        }
    }
}