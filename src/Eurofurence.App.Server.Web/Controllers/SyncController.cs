using System;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Knowledge;
using Eurofurence.App.Domain.Model.Sync;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Announcements;
using Eurofurence.App.Server.Services.Abstractions.Dealers;
using Eurofurence.App.Server.Services.Abstractions.Events;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Server.Services.Abstractions.Knowledge;
using Eurofurence.App.Server.Services.Abstractions.Maps;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/[controller]")]
    public class SyncController : BaseController
    {
        private readonly IAnnouncementService _announcementService;
        private readonly IDealerService _dealerService;
        private readonly IEventConferenceDayService _eventConferenceDayService;
        private readonly IEventConferenceRoomService _eventConferenceRoomService;
        private readonly IEventConferenceTrackService _eventConferenceTrackService;
        private readonly IEventService _eventService;
        private readonly IImageService _imageService;
        private readonly IKnowledgeEntryService _knowledgeEntryService;
        private readonly IKnowledgeGroupService _knowledgeGroupService;
        private readonly ILogger _logger;
        private readonly IMapService _mapService;
        private readonly ConventionSettings _conventionSettings;
        private readonly IMapper _mapper;
        public SyncController(
            ILoggerFactory loggerFactory,
            IEventService eventService,
            IEventConferenceDayService eventConferenceDayService,
            IEventConferenceRoomService eventConferenceRoomService,
            IEventConferenceTrackService eventConferenceTrackService,
            IKnowledgeGroupService knowledgeGroupService,
            IKnowledgeEntryService knowledgeEntryService,
            IImageService imageService,
            IDealerService dealerService,
            IAnnouncementService announcementService,
            IMapService mapService,
            ConventionSettings conventionSettings,
            IMapper mapper
        )
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _eventConferenceTrackService = eventConferenceTrackService;
            _eventConferenceRoomService = eventConferenceRoomService;
            _eventConferenceDayService = eventConferenceDayService;
            _eventService = eventService;
            _knowledgeGroupService = knowledgeGroupService;
            _knowledgeEntryService = knowledgeEntryService;
            _imageService = imageService;
            _dealerService = dealerService;
            _announcementService = announcementService;
            _mapService = mapService;
            _conventionSettings = conventionSettings;
            _mapper = mapper;
        }

        /// <summary>
        ///     Returns everything you could ever wish for.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 10, Location=ResponseCacheLocation.Any)]
        public async Task<AggregatedDeltaResponse> GetDeltaAsync([FromQuery] DateTime? since = null)
        {
            _logger.LogInformation("Execute=Sync, Since={since}", since);

            var response = new AggregatedDeltaResponse
            {
                ConventionIdentifier = _conventionSettings.ConventionIdentifier,
                State = _conventionSettings.State,
                Since = since,
                CurrentDateTimeUtc = DateTime.UtcNow,

                Events = await _eventService.GetDeltaResponseAsync(since),
                EventConferenceDays = await _eventConferenceDayService.GetDeltaResponseAsync(since),
                EventConferenceRooms = await _eventConferenceRoomService.GetDeltaResponseAsync(since),
                EventConferenceTracks = await _eventConferenceTrackService.GetDeltaResponseAsync(since),
                KnowledgeGroups = await _knowledgeGroupService.GetDeltaResponseAsync(since),
                KnowledgeEntries = _mapper.Map<DeltaResponse<KnowledgeEntryResponse>>(await _knowledgeEntryService.GetDeltaResponseAsync(since)),
                Images = await _imageService.GetDeltaResponseAsync(since),
                Dealers = await _dealerService.GetDeltaResponseAsync(since),
                Announcements = await _announcementService.GetDeltaResponseAsync(since),
                Maps = await _mapService.GetDeltaResponseAsync(since)
            };

            return response;
        }
    }
}