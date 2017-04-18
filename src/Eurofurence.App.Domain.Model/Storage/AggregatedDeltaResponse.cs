using System;

namespace Eurofurence.App.Domain.Model.Sync
{
    public class AggregatedDeltaResponse
    {
        public DateTime? Since { get; set; }
        public DateTime CurrentDateTimeUtc { get; set; }

        public DeltaResponse<Events.EventRecord> Events { get; set; }
        public DeltaResponse<Events.EventConferenceDayRecord> EventConferenceDays { get; set; }
        public DeltaResponse<Events.EventConferenceRoomRecord> EventConferenceRooms { get; set; }
        public DeltaResponse<Events.EventConferenceTrackRecord> EventConferenceTracks { get; set; }
        public DeltaResponse<Knowledge.KnowledgeGroupRecord> KnowledgeGroups { get; set; }
        public DeltaResponse<Knowledge.KnowledgeEntryRecord> KnowledgeEntries { get; set; }
    }


    public class DeltaResponse<T> where T: EntityBase
    {
        public DateTime StorageLastChangeDateTimeUtc { get; set; }
        public DateTime StorageDeltaStartChangeDateTimeUtc { get; set; }

        public bool RemoveAllBeforeInsert { get; set; }

        public T[] ChangedEntities { get; set; }
        public Guid[] DeletedEntities { get; set; }
    }
}
