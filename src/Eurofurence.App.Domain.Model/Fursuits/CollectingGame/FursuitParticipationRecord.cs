using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Eurofurence.App.Domain.Model.CollectionGame;

namespace Eurofurence.App.Domain.Model.Fursuits.CollectingGame
{
    public class FursuitParticipationRecord : EntityBase
    {
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

        [JsonIgnore]
        public IList<CollectionEntryRecord> CollectionEntries { get; set; } = new List<CollectionEntryRecord>();
    }
}
