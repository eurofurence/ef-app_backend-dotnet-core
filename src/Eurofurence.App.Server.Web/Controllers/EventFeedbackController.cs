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

        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> PostEventFeedbackAsync([FromBody]PostEventFeedbackRequest request)
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
    }
}