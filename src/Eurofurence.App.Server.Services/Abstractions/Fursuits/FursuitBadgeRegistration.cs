using System;

namespace Eurofurence.App.Server.Services.Abstractions.Fursuits
{
    public class FursuitBadgeRegistration
    {
        public int BadgeNo { get; set; }
        public int RegNo { get; set; }
        public string Name { get; set; }
        public string WornBy { get; set; }
        public string Species { get; set; }
        public string Gender { get; set; }
        public Guid ImageId { get; set; }
        public int DontPublish { get; set; }
        public string CollectionCode { get; set; }
    }
}