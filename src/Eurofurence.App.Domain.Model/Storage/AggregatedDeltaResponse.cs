using System;
using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Domain.Model.Dealers;
using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Domain.Model.Images;
using Eurofurence.App.Domain.Model.Knowledge;
using Eurofurence.App.Domain.Model.Maps;

namespace Eurofurence.App.Domain.Model.Sync
{
    public class AggregatedDeltaResponse
    {
        public string ConventionIdentifier { get; set; }

        public DateTime? Since { get; set; }
        public DateTime CurrentDateTimeUtc { get; set; }
        public string State { get; set; }

        public DeltaResponse<EventResponse> Events { get; set; }
        public DeltaResponse<EventConferenceDayResponse> EventConferenceDays { get; set; }
        public DeltaResponse<EventConferenceRoomResponse> EventConferenceRooms { get; set; }
        public DeltaResponse<EventConferenceTrackResponse> EventConferenceTracks { get; set; }
        public DeltaResponse<KnowledgeGroupResponse> KnowledgeGroups { get; set; }
        public DeltaResponse<KnowledgeEntryResponse> KnowledgeEntries { get; set; }
        public DeltaResponse<ImageResponse> Images { get; set; }
        public DeltaResponse<DealerResponse> Dealers { get; set; }
        public DeltaResponse<AnnouncementResponse> Announcements { get; set; }
        public DeltaResponse<MapResponse> Maps { get; set; }
    }


    public class DeltaResponse<T> where T : ResponseBase
    {
        public DateTime StorageLastChangeDateTimeUtc { get; set; }
        public DateTime StorageDeltaStartChangeDateTimeUtc { get; set; }

        public bool RemoveAllBeforeInsert { get; set; }

        public T[] ChangedEntities { get; set; }
        public Guid[] DeletedEntities { get; set; }
    }
}