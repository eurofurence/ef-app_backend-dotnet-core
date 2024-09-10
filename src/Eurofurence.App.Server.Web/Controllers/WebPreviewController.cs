using Eurofurence.App.Domain.Model.Maps;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Dealers;
using Eurofurence.App.Server.Services.Abstractions.Events;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Server.Services.Abstractions.Knowledge;
using Eurofurence.App.Server.Services.Abstractions.Maps;
using Eurofurence.App.Server.Web.Extensions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
        private readonly IKnowledgeGroupService _knowledgeGroupService;
        private readonly IKnowledgeEntryService _knowledgeEntryService;
        private readonly IMapService _mapService;
        private readonly IImageService _imageService;
        private ILogger _logger;

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
            IDealerService dealerService,
            IKnowledgeGroupService knowledgeGroupService,
            IKnowledgeEntryService knowledgeEntryService,
            IMapService mapService,
            IImageService imageService,
            ILoggerFactory loggerFactory)
        {
            _conventionSettings = conventionSettings;
            _eventService = eventService;
            _eventConferenceDayService = eventConferenceDayService;
            _eventConferenceRoomService = eventConferenceRoomService;
            _eventConferenceTrackService = eventConferenceTrackService;
            _dealerService = dealerService;
            _knowledgeGroupService = knowledgeGroupService;
            _knowledgeEntryService = knowledgeEntryService;
            _mapService = mapService;
            _imageService = imageService;
            _logger = loggerFactory.CreateLogger(GetType());
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

        [HttpGet("Events/{id}")]
        public async Task<ActionResult> GetEventById(Guid id)
        {
            var @event = await _eventService.FindAll().Include(e => e.BannerImage).Include(e => e.PosterImage).FirstOrDefaultAsync(e => e.Id == id);
            if (@event == null) return NotFound();

            var previewImageUrl = @event.PosterImage?.Url ?? @event.BannerImage?.Url ?? string.Empty;

            var eventConferenceDay = await _eventConferenceDayService.FindOneAsync(@event.ConferenceDayId);
            var eventConferenceRoom = await _eventConferenceRoomService.FindOneAsync(@event.ConferenceRoomId);
            var eventConferenceTrack = await _eventConferenceTrackService.FindOneAsync(@event.ConferenceTrackId);

            PopulateViewData();

            ViewData[VIEWDATA_OPENGRAPH_METADATA] = new OpenGraphMetadata()
                .WithTitle(@event.Title)
                .WithDescription($"{eventConferenceDay.Date.DayOfWeek} ({eventConferenceDay.Name}) {@event.StartTime:hh\\:mm}-{@event.EndTime:hh\\:mm}, {eventConferenceRoom.Name}\n\n{@event.Description}")
                .WithImage(previewImageUrl);

            ViewData["eventConferenceDay"] = eventConferenceDay;
            ViewData["eventConferenceRoom"] = eventConferenceRoom;
            ViewData["eventConferenceTrack"] = eventConferenceTrack;

            return View("EventPreview", @event);
        }

        [HttpGet("Events")]
        public ActionResult GetEvents()
        {
            ViewData[VIEWDATA_OPENGRAPH_METADATA] = new OpenGraphMetadata()
                            .WithTitle($"Eurofurence {_conventionSettings.ConventionNumber} Event Schedule")
                            .WithDescription($"Find out what is happening when and where at Eurofurence {_conventionSettings.ConventionNumber}");

            return View("EventsPreview", _conventionSettings);
        }

        [HttpGet("Dealers/{id}")]
        public async Task<ActionResult> GetDealerById(Guid id)
        {
            var dealer = await _dealerService.FindAll().Include(d => d.ArtistImage).Include(d => d.ArtistThumbnailImage).Include(d => d.ArtPreviewImage).FirstOrDefaultAsync(d => d.Id == id);
            if (dealer == null) return NotFound();


            var maps = _mapService.FindAll().Include(m => m.Image);
            var mapEntries = new List<MapEntryRecord>();

            foreach (var map in maps)
            {
                foreach (var entry in map.Entries.Where(
                                entry => entry.Links.Any(
                                    link => link.FragmentType == Domain.Model.Fragments.LinkFragment.FragmentTypeEnum.DealerDetail
                                        && link.Target.Equals(dealer.Id.ToString(), StringComparison.CurrentCultureIgnoreCase))))
                {
                    entry.Map = map;
                    mapEntries.Add(entry);
                }
            }

            ViewData["MapEntries"] = mapEntries;

            PopulateViewData();

            var previewImageUrl = dealer.ArtistImage?.Url ?? dealer.ArtistImage?.Url ?? string.Empty;

            ViewData[VIEWDATA_OPENGRAPH_METADATA] = new OpenGraphMetadata()
                .WithTitle(dealer.DisplayNameOrAttendeeNickname)
                .WithDescription(dealer.ShortDescription)
                .WithImage(previewImageUrl);

            return View("DealerPreview", dealer);
        }

        [HttpGet("KnowledgeGroups")]
        public async Task<ActionResult> GetKnowledgeGroups()
        {
            var knowledgeGroups = await _knowledgeGroupService.FindAll().ToListAsync();
            var knowledgeEntries = await _knowledgeEntryService.FindAll().ToListAsync();

            PopulateViewData();

            ViewData[VIEWDATA_OPENGRAPH_METADATA] = new OpenGraphMetadata()
                .WithTitle("Knowledge Base")
                .WithDescription("Helpful information across all areas & departments");

            ViewData["knowledgeEntries"] = knowledgeEntries;
            return View("KnowledgeGroupsPreview", knowledgeGroups);
        }

        [HttpGet("KnowledgeEntries/{id}")]
        public async Task<ActionResult> GetKnowledgeEntryById(Guid id)
        {
            var knowledgeEntry = await _knowledgeEntryService.FindOneAsync(id);
            var knowledgeGroup = await _knowledgeGroupService.FindOneAsync(knowledgeEntry.KnowledgeGroupId);

            PopulateViewData();

            ViewData[VIEWDATA_OPENGRAPH_METADATA] = new OpenGraphMetadata()
                .WithTitle(knowledgeEntry.Title)
                .WithDescription(knowledgeGroup.Name)
                .WithImage(knowledgeEntry.Images?.FirstOrDefault()?.Url ?? string.Empty);

            ViewData["knowledgeGroup"] = knowledgeGroup;
            return View("KnowledgeEntryPreview", knowledgeEntry);
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