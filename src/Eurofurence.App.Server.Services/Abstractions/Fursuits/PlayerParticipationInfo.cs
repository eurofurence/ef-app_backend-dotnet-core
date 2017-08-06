using System;
using System.Collections.Generic;

namespace Eurofurence.App.Server.Services.Abstractions.Fursuits
{
    public class PlayerParticipationInfo
    {
        public string Name { get; set; }
        public bool IsBanned { get; set; }
        public int CollectionCount { get; set; }
        public int? ScoreboardRank { get; set; }

        public IList<BadgeInfo> RecentlyCollected { get; set; }

        public class BadgeInfo
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }

        public PlayerParticipationInfo()
        {
            RecentlyCollected = new List<BadgeInfo>();
        }
    }
}