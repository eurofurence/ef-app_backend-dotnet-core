using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eurofurence.App.Common.Results;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Fursuits;
using Eurofurence.App.Domain.Model.Fursuits.CollectingGame;
using Eurofurence.App.Domain.Model.Security;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Fursuits;
using Eurofurence.App.Server.Services.Security;

namespace Eurofurence.App.Server.Services.Fursuits
{
    public class CollectingGameService:  ICollectingGameService
    {
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private readonly ConventionSettings _conventionSettings;
        private readonly IEntityRepository<FursuitBadgeRecord> _fursuitBadgeRepository;
        private readonly IEntityRepository<FursuitParticipationRecord> _fursuitParticipationRepository;
        private readonly IEntityRepository<PlayerParticipationRecord> _playerParticipationRepository;
        private readonly IEntityRepository<RegSysIdentityRecord> _regSysIdentityRepository;
        private readonly IEntityRepository<TokenRecord> _tokenRepository;

        public CollectingGameService(
            ConventionSettings conventionSettings,
            IEntityRepository<FursuitBadgeRecord> fursuitBadgeRepository,
            IEntityRepository<FursuitParticipationRecord> fursuitParticipationRepository,
            IEntityRepository<PlayerParticipationRecord> playerParticipationRepository,
            IEntityRepository<RegSysIdentityRecord> regSysIdentityRepository,
            IEntityRepository<TokenRecord> tokenRepository
            )
        {
            _conventionSettings = conventionSettings;
            _fursuitBadgeRepository = fursuitBadgeRepository;
            _fursuitParticipationRepository = fursuitParticipationRepository;
            _playerParticipationRepository = playerParticipationRepository;
            _regSysIdentityRepository = regSysIdentityRepository;
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

        public async Task<PlayerParticipationInfo> GetPlayerParticipationInfoForPlayerAsync(string playerUid, string playerName)
        {
            var response = new PlayerParticipationInfo() {Name = playerName};
            var playerParticipation = await _playerParticipationRepository.FindOneAsync(a => a.PlayerUid == playerUid);

            if (playerParticipation != null)
            {
                response.CollectionCount = playerParticipation.CollectionCount;

                var fetchRecentlyCollectedAsync = playerParticipation.CollectionEntries
                    .OrderByDescending(a => a.EventDateTimeUtc)
                    .Take(5)
                    .Select(async entry =>
                    {
                        var fursuitParticipation =
                            await _fursuitParticipationRepository.FindOneAsync(entry.FursuitParticipationUid);
                        var badge = await _fursuitBadgeRepository.FindOneAsync(fursuitParticipation.FursuitBadgeId);

                        return new PlayerParticipationInfo.BadgeInfo
                        {
                            Id = badge.Id,
                            Name = badge.Name
                        };
                    })
                    .ToList();


                var playersAhead = await _playerParticipationRepository.FindAllAsync(
                        a => !a.IsBanned && a.PlayerUid != playerParticipation.PlayerUid
                        && a.CollectionCount >= playerParticipation.CollectionCount);

                var rank = playersAhead.Count(a => a.CollectionCount > playerParticipation.CollectionCount
                                                   || (a.CollectionCount == playerParticipation.CollectionCount &&
                                                       a.CollectionEntries.Max(b => b.EventDateTimeUtc) <
                                                       playerParticipation.CollectionEntries.Max(b => b.EventDateTimeUtc)
                                                   )) + 1;
                response.ScoreboardRank = rank;

                await Task.WhenAll(fetchRecentlyCollectedAsync);

                response.RecentlyCollected = fetchRecentlyCollectedAsync.Select(task => task.Result).ToList();
            }

            return response;
        }

        public async Task<IResult> RegisterTokenForFursuitBadgeForOwnerAsync(string ownerUid, Guid fursuitBadgeId, string tokenValue)
        {
            try
            {
                await _semaphore.WaitAsync();

                var fursuitBadge = await _fursuitBadgeRepository.FindOneAsync(
                    badge => badge.OwnerUid == ownerUid && badge.Id == fursuitBadgeId);
                if (fursuitBadge == null) return Result.Error("INVALID_FURSUIT_BADGE_ID", "Invalid fursuitBadgeId");

                var token = await _tokenRepository.FindOneAsync(t => t.Value == tokenValue && t.IsLinked == false);
                if (token == null) return Result.Error("INVALID_TOKEN", "Invalid token");

                var existingParticipation =
                    await _fursuitParticipationRepository.FindOneAsync(p => p.FursuitBadgeId == fursuitBadgeId);
                if (existingParticipation != null) return Result.Error("EXISTING_PARTICIPATION", "Fursuit already has a token assigned to it");

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

                return Result.Ok;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<IResult<CollectTokenResponse>> CollectTokenForPlayerAsync(string playerUid, string tokenValue)
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
                    return Result<CollectTokenResponse>.Error("BANNED", "You have been disqualified from the game.");
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
                            return Result<CollectTokenResponse>.Error("INVALID_TOKEN_BAN_IMMINENT", sb.ToString());
                        }
                        else
                        {
                            sb.Append(" You have been disqualified.");
                            return Result<CollectTokenResponse>.Error("INVALID_TOKEN_BANNED", sb.ToString());
                        }
                    }

                    return Result<CollectTokenResponse>.Error("INVALID_TOKEN", sb.ToString());
                }

                if (playerParticipation.PlayerUid == fursuitParticipation.OwnerUid)
                    return Result<CollectTokenResponse>.Error("INVALID_TOKEN_OWN_SUIT", "You cannot collect your own fursuits.");

                if (playerParticipation.CollectionEntries.Any(a => a.FursuitParticipationUid == fursuitParticipation.Id))
                    return Result<CollectTokenResponse>.Error("INVALID_TOKEN_ALREADY_COLLECTED", "You have already collected this suit!");

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

                return Result<CollectTokenResponse>.Ok(new CollectTokenResponse()
                {
                    FursuitBadgeId = fursuitBadge.Id,
                    Name = fursuitBadge.Name,
                    Gender = fursuitBadge.Gender,
                    Species = fursuitBadge.Species,
                    FursuitCollectionCount = fursuitParticipation.CollectionCount
                });
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<IResult<PlayerScoreboardEntry[]>> GetPlayerScoreboardEntriesAsync(int top)
        {
            var players = await _playerParticipationRepository.FindAllAsync(a => !a.IsBanned && a.IsDeleted == 0);

            var tasks = players
                .OrderByDescending(a => a.CollectionCount)
                .ThenBy(a => a.CollectionEntries.Max(b => b.EventDateTimeUtc))
                .Take(top)
                .Select(async a => new PlayerScoreboardEntry()
                {
                    Name = (await _regSysIdentityRepository.FindOneAsync(b => b.Uid == a.PlayerUid))?.Username ?? "(?)",
                    CollectionCount = a.CollectionCount,
                })
                .ToList();

            await Task.WhenAll(tasks);

            for (var i = 0; i < tasks.Count; i++) tasks[i].Result.Rank = i + 1;

            return Result<PlayerScoreboardEntry[]>.Ok(tasks.Select(a => a.Result).ToArray());
        }

        public async Task<IResult<FursuitScoreboardEntry[]>> GetFursuitScoreboardEntriesAsync(int top)
        {
            var fursuits = await _fursuitParticipationRepository.FindAllAsync(a => !a.IsBanned && a.IsDeleted == 0);

            var tasks = fursuits
                .OrderByDescending(a => a.CollectionCount)
                .ThenBy(a => a.CollectionEntries.Max(b => b.EventDateTimeUtc))
                .Take(top)
                .Select(async a =>
                {
                    var fursuitBadge = await _fursuitBadgeRepository
                        .FindOneAsync(b => b.Id == a.FursuitBadgeId);

                    return new FursuitScoreboardEntry()
                    {
                        Name = fursuitBadge.Name,
                        CollectionCount = a.CollectionCount,
                        Gender = fursuitBadge.Gender,
                        Species = fursuitBadge.Species,
                        BadgeId = fursuitBadge.Id
                    };
                })
                .ToList();

            await Task.WhenAll(tasks);

            for (var i = 0; i < tasks.Count; i++) tasks[i].Result.Rank = i + 1;

            return Result<FursuitScoreboardEntry[]>
                .Ok(tasks.Select(a => a.Result).ToArray());
        }
    }
}
