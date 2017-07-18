using Eurofurence.App.Domain.Model.Fursuits;
using Eurofurence.App.Domain.Model.Fursuits.CollectingGame;

namespace Eurofurence.App.Server.Services.Abstractions.Fursuits
{
    public class FursuitParticipationInfo
    {
        public FursuitBadgeRecord Badge { get; set; }
        public bool IsParticipating { get; set; }
        public FursuitParticipationRecord Participation { get; set; }
    }
}