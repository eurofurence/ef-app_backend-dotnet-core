using System;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Fursuits;
using Eurofurence.App.Domain.Model.Fursuits.CollectingGame;

namespace Eurofurence.App.Server.Services.Abstractions.Fursuits
{
    public interface ICollectingGameService
    {
        Task<FursuitParticipationInfo[]> GetFursuitParticipationInfoForOwnerAsync(string ownerUid);
        Task<PlayerParticipationInfo> GetPlayerParticipationInfoForPlayerAsync(string playerUid);
        Task<bool> LinkTokenToFursuitBadge(string ownerUid, Guid fursuitBadgeId, string tokenValue);

        Task<CollectTokenResponse> CollectTokenForPlayerAsync(string playerUid, string tokenValue);
    }

    public class CollectTokenResponse
    {
        public bool IsSuccessful { get; set; }
        public string FailureMessage { get; set; }

        public Guid? FursuitBadgeId { get; set; }
        public int FursuitCollectionCount { get; set; }

        public string Name { get; set; }
        public string Species { get; set; }
        public string Gender { get; set; }
    }

    public class PlayerParticipationInfo
    {
        public int CollectionCount { get; set; }
        public int? ScoreboardRank { get; set; }
    }

    public class FursuitParticipationInfo
    {
        public FursuitBadgeRecord Badge { get; set; }
        public bool IsParticipating { get; set; }
        public FursuitParticipationRecord Participation { get; set; }
    }
}
