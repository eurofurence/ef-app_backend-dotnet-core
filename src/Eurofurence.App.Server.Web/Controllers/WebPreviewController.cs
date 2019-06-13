using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Dealers;
using Eurofurence.App.Server.Services.Abstractions.Events;
using Eurofurence.App.Server.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Web")]
    public class WebPreviewController : BaseController
    {
        private readonly ConventionSettings _conventionSettings;
        private readonly IEventService _eventService;
        private readonly IEventConferenceDayService _eventConferenceDayService;
        private readonly IDealerService _dealerService;

        public WebPreviewController(
            ConventionSettings conventionSettings,
            IEventService eventService, 
            IEventConferenceDayService eventConferenceDayService,
            IDealerService dealerService
            )
        {
            _conventionSettings = conventionSettings;
            _eventService = eventService;
            _eventConferenceDayService = eventConferenceDayService;
            _dealerService = dealerService;
        }

        [HttpGet("Events/{Id}")]
        public async Task<ActionResult> GetEventById(Guid Id)
        {
            var @event = await _eventService.FindOneAsync(Id);
            if (@event == null) return NotFound();

            var eventConferenceDay = await _eventConferenceDayService.FindOneAsync(@event.ConferenceDayId);

            return new WebPreviewMetadata()
                .WithTitle(@event.Title)
                .WithDescription($"{eventConferenceDay.Name} {@event.StartTime}-{@event.EndTime}\n{@event.Description}")
                .AsViewResult();
        }

        [HttpGet("Dealers/{Id}")]
        public async Task<ActionResult> GetDealerById(Guid Id)
        {
            var dealer = await _dealerService.FindOneAsync(Id);
            if (dealer == null) return NotFound();

            return new WebPreviewMetadata()
                .WithTitle(string.IsNullOrEmpty(dealer.DisplayName) ? dealer.AttendeeNickname : dealer.DisplayName)
                .WithDescription(dealer.ShortDescription)
                .WithImage(dealer.ArtistImageId.HasValue ? $"{_conventionSettings.ApiBaseUrl}Images/{dealer.ArtistImageId}/Content" : string.Empty)
                .AsViewResult();
        }

    }
}
