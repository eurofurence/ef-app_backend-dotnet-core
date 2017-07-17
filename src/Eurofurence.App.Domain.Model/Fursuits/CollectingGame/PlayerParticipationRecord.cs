using System;
using System.Collections.Generic;

namespace Eurofurence.App.Domain.Model.Fursuits.CollectingGame
{
    public class PlayerParticipationRecord : EntityBase
    {
        public class CollectionEntry
        {
            public Guid FursuitParticipationUid { get; set; }
            public DateTime EventDateTimeUtc { get; set; }
        }

        public string PlayerUid { get; set; }

        public int Karma { get; set; }
        public bool IsBanned { get; set; }

        public int CollectionCount { get; set; }
        public IList<CollectionEntry> CollectionEntries { get; set; }

        public PlayerParticipationRecord()
        {
            CollectionEntries = new List<CollectionEntry>();
        }
    }
}
