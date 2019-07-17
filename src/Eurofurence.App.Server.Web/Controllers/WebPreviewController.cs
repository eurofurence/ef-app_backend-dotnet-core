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
        private readonly IEventConferenceRoomService _eventConferenceRoomService;
        private readonly IEventConferenceTrackService _eventConferenceTrackService;
        private readonly IDealerService _dealerService;

        public const string VIEWDATA_OPENGRAPH_METADATA = nameof(OpenGraphMetadata);
        public const string VIEWDATA_APPID_ITUNES = nameof(ConventionSettings.AppIdITunes);
        public const string VIEWDATA_APPID_PLAY = nameof(ConventionSettings.AppIdPlay);
        public const string VIEWDATA_BASE_URL = nameof(ConventionSettings.BaseUrl);
        public const string VIEWDATA_API_BASE_URL = nameof(ConventionSettings.ApiBaseUrl);
        public const string VIEWDATA_CONTENT_BASE_URL = nameof(ConventionSettings.ContentBaseUrl);
        public const string VIEWDATA_WEB_BASE_URL = nameof(ConventionSettings.WebBaseUrl);

        public WebPreviewController(
            ConventionSettings conventionSettings,
            IEventService eventService,
            IEventConferenceDayService eventConferenceDayService,
            IEventConferenceRoomService eventConferenceRoomService,
            IEventConferenceTrackService eventConferenceTrackService,
            IDealerService dealerService
            )
        {
            _conventionSettings = conventionSettings;
            _eventService = eventService;
            _eventConferenceDayService = eventConferenceDayService;
            _eventConferenceRoomService = eventConferenceRoomService;
            _eventConferenceTrackService = eventConferenceTrackService;
            _dealerService = dealerService;
        }

        private void PopulateViewData()
        {
            ViewData[VIEWDATA_API_BASE_URL] = _conventionSettings.ApiBaseUrl;
            ViewData[VIEWDATA_BASE_URL] = _conventionSettings.BaseUrl;
            ViewData[VIEWDATA_WEB_BASE_URL] = _conventionSettings.WebBaseUrl;
            ViewData[VIEWDATA_CONTENT_BASE_URL] = _conventionSettings.ContentBaseUrl;
            ViewData[VIEWDATA_APPID_ITUNES] = _conventionSettings.AppIdITunes;
            ViewData[VIEWDATA_APPID_PLAY] = _conventionSettings.AppIdPlay;
        }

        [HttpGet("Events/{Id}")]
        public async Task<ActionResult> GetEventById(Guid Id)
        {
            var @event = await _eventService.FindOneAsync(Id);
            if (@event == null) return NotFound();

            var previewImageId = @event.PosterImageId ?? @event.BannerImageId ?? null;

            var eventConferenceDay = await _eventConferenceDayService.FindOneAsync(@event.ConferenceDayId);
            var eventConferenceRoom = await _eventConferenceRoomService.FindOneAsync(@event.ConferenceRoomId);
            var eventConferenceTrack = await _eventConferenceTrackService.FindOneAsync(@event.ConferenceTrackId);

            PopulateViewData();

            ViewData[VIEWDATA_OPENGRAPH_METADATA] = new OpenGraphMetadata()
                .WithTitle(@event.Title)
                .WithDescription($"{eventConferenceDay.Date.DayOfWeek} ({eventConferenceDay.Name}) {@event.StartTime.ToString("hh\\:mm")}-{@event.EndTime.ToString("hh\\:mm")}, {eventConferenceRoom.Name}\n\n{@event.Description}")
                .WithImage(previewImageId.HasValue ? $"{_conventionSettings.ApiBaseUrl}/Images/{previewImageId}/Content" : string.Empty);

            ViewData["eventConferenceDay"] = eventConferenceDay;
            ViewData["eventConferenceRoom"] = eventConferenceRoom;
            ViewData["eventConferenceTrack"] = eventConferenceTrack;

            return View("EventPreview", @event);
        }

        [HttpGet("Dealers/{Id}")]
        public async Task<ActionResult> GetDealerById(Guid Id)
        {
            var dealer = await _dealerService.FindOneAsync(Id);
            if (dealer == null) return NotFound();

            PopulateViewData();

            var previewImageId = dealer.ArtistImageId ?? dealer.ArtistImageId ?? null;

            ViewData[VIEWDATA_OPENGRAPH_METADATA] = new OpenGraphMetadata()
                .WithTitle(string.IsNullOrEmpty(dealer.DisplayName) ? dealer.AttendeeNickname : dealer.DisplayName)
                .WithDescription(dealer.ShortDescription)
                .WithImage(previewImageId.HasValue ? $"{_conventionSettings.ApiBaseUrl}/Images/{previewImageId}/Content" : string.Empty);

            return View("DealerPreview", dealer);
        }

        [HttpGet("manifest.json")]
        public ActionResult GetManifest()
        {
            return Json(new
            {
                short_name = "Eurofurence",
                name = "Eurofurence",
                icons = new[] {
                    new {
                        //TODO: provide actual icon for Android app
                        src = "/images/icon192.png",
                        type = "image/png",
                        sizes = "192x192"
                    },
                    new {
                        //TODO: provide actual icon for Android app
                        src = "/images/icon512.png",
                        type = "image/png",
                        sizes = "512x512"
                    }
                },
                prefer_related_applications = true,
                related_applications = new[] {
                    new {
                        platform = "play",
                        id = _conventionSettings.AppIdPlay
                    }
                }
            });
        }

    }
}