using System;
using System.Collections.Generic;

namespace Eurofurence.App.Domain.Model.Fursuits.CollectingGame
{
    public class FursuitParticipantRecord : EntityBase
    {
        public class CollectionEntry
        {
            public string PlayerParticipantUid { get; set; }
            public DateTime EventDateTimeUtc { get; set; }
        }

        public string OwnerUid { get; set; }
        public Guid FursuitBadgeId { get; set; }

        public string Token { get; set; }
        public bool IsBanned { get; set; }

        public DateTime TokenRegistrationDateTimeUtc { get; set; }

        public int CollectionCount { get; set; }
        public IList<CollectionEntry> CollectionEntries { get; set; }

        public FursuitParticipantRecord()
        {
            CollectionEntries = new List<CollectionEntry>();
        }
    }
}
