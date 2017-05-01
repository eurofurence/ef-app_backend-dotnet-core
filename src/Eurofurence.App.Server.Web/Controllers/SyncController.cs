using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Eurofurence.App.Domain.Model.Sync;
using Eurofurence.App.Server.Services.Abstractions;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/v2/[controller]")]
    public class SyncController
    {
        readonly IEventService _eventService;
        readonly IEventConferenceDayService _eventConferenceDayService;
        readonly IEventConferenceRoomService _eventConferenceRoomService;
        readonly IEventConferenceTrackService _eventConferenceTrackService;
        readonly IKnowledgeGroupService _knowledgeGroupService;
        readonly IKnowledgeEntryService _knowledgeEntryService;
        readonly IImageService _imageService;
        readonly IDealerService _dealerService;

        public SyncController(
            IEventService eventService,
            IEventConferenceDayService eventConferenceDayService,
            IEventConferenceRoomService eventConferenceRoomService,
            IEventConferenceTrackService eventConferenceTrackService,
            IKnowledgeGroupService knowledgeGroupService,
            IKnowledgeEntryService knowledgeEntryService,
            IImageService imageService,
            IDealerService dealerService
            )
        {
            _eventConferenceTrackService = eventConferenceTrackService;
            _eventConferenceRoomService = eventConferenceRoomService;
            _eventConferenceDayService = eventConferenceDayService;
            _eventService = eventService;
            _knowledgeGroupService = knowledgeGroupService;
            _knowledgeEntryService = knowledgeEntryService;
            _imageService = imageService;
            _dealerService = dealerService;
        }

        /// <summary>
        /// Returns everything you could ever wish for.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<AggregatedDeltaResponse> GetDeltaAsync(DateTime? since = null)
        {
            var response = new AggregatedDeltaResponse()
            {
                Since = since,
                CurrentDateTimeUtc = DateTime.UtcNow,

                Events = await _eventService.GetDeltaResponseAsync(minLastDateTimeChangedUtc: since),
                EventConferenceDays = await _eventConferenceDayService.GetDeltaResponseAsync(minLastDateTimeChangedUtc: since),
                EventConferenceRooms = await _eventConferenceRoomService.GetDeltaResponseAsync(minLastDateTimeChangedUtc: since),
                EventConferenceTracks = await _eventConferenceTrackService.GetDeltaResponseAsync(minLastDateTimeChangedUtc: since),
                KnowledgeGroups = await _knowledgeGroupService.GetDeltaResponseAsync(minLastDateTimeChangedUtc: since),
                KnowledgeEntries = await _knowledgeEntryService.GetDeltaResponseAsync(minLastDateTimeChangedUtc: since),
                Images = await _imageService.GetDeltaResponseAsync(minLastDateTimeChangedUtc: since),
                Dealers = await _dealerService.GetDeltaResponseAsync(minLastDateTimeChangedUtc: since)
            };

            return response;
        }
    }
}
