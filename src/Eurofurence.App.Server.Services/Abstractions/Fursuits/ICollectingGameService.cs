using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eurofurence.App.Common.Results;

namespace Eurofurence.App.Server.Services.Abstractions.Fursuits
{
    public interface ICollectingGameService
    {
        Task<IEnumerable<FursuitParticipationInfo>> GetFursuitParticipationInfoForOwnerAsync(string ownerUid);
        Task<PlayerParticipationInfo> GetPlayerParticipationInfoForPlayerAsync(string playerUid, string playerName);
        Task<PlayerCollectionEntry[]> GetPlayerCollectionEntriesForPlayerAsync(string playerUid);

        Task<IResult> RegisterTokenForFursuitBadgeForOwnerAsync(string ownerUid, Guid fursuitBadgeId, string tokenValue);
        Task<IResult<CollectTokenResponse>> CollectTokenForPlayerAsync(string identityId, string tokenValue, string username);

        Task<IResult<PlayerScoreboardEntry[]>>  GetPlayerScoreboardEntriesAsync(int top);
        Task<IResult<FursuitScoreboardEntry[]>> GetFursuitScoreboardEntriesAsync(int top);

        Task<IResult> CreateTokenFromValueAsync(string tokenValue);
        Task<IResult> CreateTokensFromValuesAsync(string[] tokenValues);

        Task<IResult> UnbanPlayerAsync(string playerUid);

        Task<IResult> RecalculateAsync();

        Task UpdateFursuitParticipationAsync();
    }
}
