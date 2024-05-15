using System;
using System.Collections.Generic;
using Eurofurence.App.Domain.Model.CollectionGame;

namespace Eurofurence.App.Domain.Model.Fursuits.CollectingGame
{
    public class PlayerParticipationRecord : EntityBase
    {
        public string PlayerUid { get; set; }

        public int Karma { get; set; }
        public bool IsBanned { get; set; }

        public DateTime? LastCollectionDateTimeUtc { get; set; }

        public int CollectionCount { get; set; }
        public IList<CollectionEntryRecord> CollectionEntries { get; set; } = new List<CollectionEntryRecord>();
    }
}
