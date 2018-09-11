using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.Fursuits.CollectingGame
{
    public class FursuitParticipationRecord : EntityBase
    {
        public class CollectionEntry
        {
            public string PlayerParticipationUid { get; set; }
            public DateTime EventDateTimeUtc { get; set; }
        }

        [DataMember]
        public string OwnerUid { get; set; }

        [DataMember]
        public Guid FursuitBadgeId { get; set; }

        [DataMember]
        public string TokenValue { get; set; }

        [DataMember]
        public bool IsBanned { get; set; }

        [DataMember]
        public DateTime TokenRegistrationDateTimeUtc { get; set; }

        [DataMember]
        public DateTime? LastCollectionDateTimeUtc { get; set; }

        [DataMember]
        public int CollectionCount { get; set; }

        [IgnoreDataMember]
        public IList<CollectionEntry> CollectionEntries { get; set; }

        public FursuitParticipationRecord()
        {
            CollectionEntries = new List<CollectionEntry>();
        }
    }
}
