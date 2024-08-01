using Eurofurence.App.Domain.Model.Fursuits.CollectingGame;

namespace Eurofurence.App.Domain.Model.Fursuits
{
    public class FursuitParticipationInfoResponse
    {
        public FursuitBadgeResponse Badge { get; set; }
        public bool IsParticipating { get; set; }
        public FursuitParticipationRecord Participation { get; set; }
    }
}
