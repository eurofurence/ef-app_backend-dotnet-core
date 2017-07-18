using System;

namespace Eurofurence.App.Server.Services.Abstractions.Fursuits
{
    public class CollectTokenResponse
    {
        public Guid? FursuitBadgeId { get; set; }
        public int FursuitCollectionCount { get; set; }

        public string Name { get; set; }
        public string Species { get; set; }
        public string Gender { get; set; }
    }
}