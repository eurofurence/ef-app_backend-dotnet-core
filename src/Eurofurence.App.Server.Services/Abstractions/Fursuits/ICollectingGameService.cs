using System;
using System.Threading.Tasks;
using Eurofurence.App.Common.Results;

namespace Eurofurence.App.Server.Services.Abstractions.Fursuits
{
    public interface ICollectingGameService
    {
        Task<FursuitParticipationInfo[]> GetFursuitParticipationInfoForOwnerAsync(string ownerUid);
        Task<PlayerParticipationInfo> GetPlayerParticipationInfoForPlayerAsync(string playerUid, string playerName);
        Task<PlayerCollectionEntry[]> GetPlayerCollectionEntriesForPlayerAsync(string playerUid);

        Task<IResult> RegisterTokenForFursuitBadgeForOwnerAsync(string ownerUid, Guid fursuitBadgeId, string tokenValue);
        Task<IResult<CollectTokenResponse>> CollectTokenForPlayerAsync(string playerUid, string tokenValue);

        Task<IResult<PlayerScoreboardEntry[]>>  GetPlayerScoreboardEntriesAsync(int top);
        Task<IResult<FursuitScoreboardEntry[]>> GetFursuitScoreboardEntriesAsync(int top);

        Task<IResult> CreateTokenFromValueAsync(string tokenValue);
        Task<IResult> CreateTokensFromValuesAsync(string[] tokenValues);
    }
}
