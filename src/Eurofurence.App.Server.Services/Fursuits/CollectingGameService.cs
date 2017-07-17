using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Fursuits;
using Eurofurence.App.Domain.Model.Fursuits.CollectingGame;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Fursuits;

namespace Eurofurence.App.Server.Services.Fursuits
{
    public class CollectingGameService:  ICollectingGameService
    {
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private readonly ConventionSettings _conventionSettings;
        private readonly IEntityRepository<FursuitBadgeRecord> _fursuitBadgeRepository;
        private readonly IEntityRepository<FursuitParticipationRecord> _fursuitParticipationRepository;
        private readonly IEntityRepository<PlayerParticipationRecord> _playerParticipationRepository;
        private readonly IEntityRepository<TokenRecord> _tokenRepository;

        public CollectingGameService(
            ConventionSettings conventionSettings,
            IEntityRepository<FursuitBadgeRecord> fursuitBadgeRepository,
            IEntityRepository<FursuitParticipationRecord> fursuitParticipationRepository,
            IEntityRepository<PlayerParticipationRecord> playerParticipationRepository,
            IEntityRepository<TokenRecord> tokenRepository
            )
        {
            _conventionSettings = conventionSettings;
            _fursuitBadgeRepository = fursuitBadgeRepository;
            _fursuitParticipationRepository = fursuitParticipationRepository;
            _playerParticipationRepository = playerParticipationRepository;
            _tokenRepository = tokenRepository;
        }


        public async Task<FursuitParticipationInfo[]> GetFursuitParticipationInfoForOwnerAsync(string ownerUid)
        {
            var fursuitBadges = await _fursuitBadgeRepository.FindAllAsync(badge => badge.OwnerUid == ownerUid);

            var results = await Task.WhenAll(fursuitBadges.Select(fursuitBadge => Task.Run(async () =>
            {
                var result = new FursuitParticipationInfo() {Badge = fursuitBadge};
                var participationRecord =
                    await _fursuitParticipationRepository.FindOneAsync(
                        fursuit => fursuit.FursuitBadgeId == fursuitBadge.Id);

                if (participationRecord != null)
                {
                    result.IsParticipating = true;
                    result.Participation = participationRecord;
                }

                return result;
            })));

            return results;
        }

        public async Task<PlayerParticipationInfo> GetPlayerParticipationInfoForPlayerAsync(string playerUid)
        {
            var response = new PlayerParticipationInfo();
            var playerParticipation = await _playerParticipationRepository.FindOneAsync(a => a.PlayerUid == playerUid);

            if (playerParticipation != null)
            {
                response.CollectionCount = playerParticipation.CollectionCount;
            }

            return response;
        }

        public async Task<bool> LinkTokenToFursuitBadge(string ownerUid, Guid fursuitBadgeId, string tokenValue)
        {
            try
            {
                await _semaphore.WaitAsync();

                var fursuitBadge = await _fursuitBadgeRepository.FindOneAsync(
                    badge => badge.OwnerUid == ownerUid && badge.Id == fursuitBadgeId);
                if (fursuitBadge == null) return false;

                var token = await _tokenRepository.FindOneAsync(t => t.Value == tokenValue && t.IsLinked == false);
                if (token == null) return false;

                var existingParticipation =
                    await _fursuitParticipationRepository.FindOneAsync(p => p.FursuitBadgeId == fursuitBadgeId);
                if (existingParticipation != null) return false;

                var newParticipation = new FursuitParticipationRecord()
                {
                    Id = Guid.NewGuid(),
                    FursuitBadgeId = fursuitBadgeId,
                    TokenValue = token.Value,
                    TokenRegistrationDateTimeUtc = DateTime.UtcNow,
                    CollectionCount = 0,
                    OwnerUid = ownerUid
                };

                token.IsLinked = true;
                token.LinkedFursuitParticipantUid = newParticipation.Id;
                token.LinkDateTimeUtc = DateTime.UtcNow;

                await _tokenRepository.ReplaceOneAsync(token);
                await _fursuitParticipationRepository.InsertOneAsync(newParticipation);

                return true;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<CollectTokenResponse> CollectTokenForPlayerAsync(string playerUid, string tokenValue)
            {
            try
            {
                await _semaphore.WaitAsync();

                var playerParticipation = await _playerParticipationRepository.FindOneAsync(a => a.PlayerUid == playerUid);
                if (playerParticipation == null)
                {
                    playerParticipation = new PlayerParticipationRecord()
                    {
                        Id = Guid.NewGuid(),
                        PlayerUid = playerUid,
                        Karma = 0
                    };
                    await _playerParticipationRepository.InsertOneAsync(playerParticipation);
                }

                if (playerParticipation.IsBanned)
                {
                    return new CollectTokenResponse {FailureMessage = "You have been disqualified from the game."};
                }

                var fursuitParticipation =
                    await _fursuitParticipationRepository.FindOneAsync(a => a.TokenValue == tokenValue);

                if (fursuitParticipation == null)
                {
                    playerParticipation.Karma -= 1;
                    if (playerParticipation.Karma <= -10)
                    {
                        playerParticipation.IsBanned = true;
                    }

                    await _playerParticipationRepository.ReplaceOneAsync(playerParticipation);
                    var sb = new StringBuilder("The token you specified is not valid.");

                    if (playerParticipation.Karma < -7)
                    {
                        if (playerParticipation.Karma > -10)
                        {
                            sb.Append(
                                $" Careful - you will be disqualified after {playerParticipation.Karma + 10} more failed attempts.");
                        }
                        else
                        {
                            sb.Append(" You have been disqualified.");
                        }
                    }

                    return new CollectTokenResponse {FailureMessage = sb.ToString()};
                }

                if (playerParticipation.PlayerUid == fursuitParticipation.OwnerUid)
                    return new CollectTokenResponse { FailureMessage = "You cannot collect your own fursuits." };

                if (playerParticipation.CollectionEntries.Any(a => a.FursuitParticipationUid == fursuitParticipation.Id))
                    return new CollectTokenResponse { FailureMessage = "You have already collected this suit!" };

                playerParticipation.Karma = Math.Min(playerParticipation.Karma + 2, 10);
                playerParticipation.CollectionEntries.Add(new PlayerParticipationRecord.CollectionEntry()
                {
                    EventDateTimeUtc = DateTime.UtcNow,
                    FursuitParticipationUid = fursuitParticipation.Id
                });

                playerParticipation.CollectionCount = playerParticipation.CollectionEntries.Count;
                await _playerParticipationRepository.ReplaceOneAsync(playerParticipation);

                // This should never *not* happen, but makes testing a bit easier.
                if (fursuitParticipation.CollectionEntries.All(a => a.PlayerParticipationUid != playerUid))
                {
                    fursuitParticipation.CollectionEntries.Add(new FursuitParticipationRecord.CollectionEntry()
                    {
                        EventDateTimeUtc = DateTime.UtcNow,
                        PlayerParticipationUid = playerUid
                    });

                    fursuitParticipation.CollectionCount = fursuitParticipation.CollectionEntries.Count;
                    await _fursuitParticipationRepository.ReplaceOneAsync(fursuitParticipation);
                }

                var fursuitBadge = await 
                    _fursuitBadgeRepository.FindOneAsync(a => a.Id == fursuitParticipation.FursuitBadgeId);

                return new CollectTokenResponse
                {
                    IsSuccessful = true,
                    FursuitBadgeId = fursuitBadge.Id,
                    Name = fursuitBadge.Name,
                    Gender = fursuitBadge.Gender,
                    Species = fursuitBadge.Species,
                    FursuitCollectionCount = fursuitParticipation.CollectionCount
                };
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
