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

        public DeltaResponse<EventRecord> Events { get; set; }
        public DeltaResponse<EventConferenceDayRecord> EventConferenceDays { get; set; }
        public DeltaResponse<EventConferenceRoomRecord> EventConferenceRooms { get; set; }
        public DeltaResponse<EventConferenceTrackRecord> EventConferenceTracks { get; set; }
        public DeltaResponse<KnowledgeGroupRecord> KnowledgeGroups { get; set; }
        public DeltaResponse<KnowledgeEntryResponse> KnowledgeEntries { get; set; }
        public DeltaResponse<ImageRecord> Images { get; set; }
        public DeltaResponse<DealerRecord> Dealers { get; set; }
        public DeltaResponse<AnnouncementRecord> Announcements { get; set; }
        public DeltaResponse<MapRecord> Maps { get; set; }
    }


    public class DeltaResponse<T> where T : EntityBase
    {
        public DateTime StorageLastChangeDateTimeUtc { get; set; }
        public DateTime StorageDeltaStartChangeDateTimeUtc { get; set; }

        public bool RemoveAllBeforeInsert { get; set; }

        public T[] ChangedEntities { get; set; }
        public Guid[] DeletedEntities { get; set; }
    }
}