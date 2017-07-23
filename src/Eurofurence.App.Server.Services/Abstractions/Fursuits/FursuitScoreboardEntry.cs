using System;

namespace Eurofurence.App.Server.Services.Abstractions.Fursuits
{
    public class FursuitScoreboardEntry : ScoreboardEntry
    {
        public Guid BadgeId { get; set; }
        public string Gender { get; set; }
        public string Species { get; set; }
    }
}