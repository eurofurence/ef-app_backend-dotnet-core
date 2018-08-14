using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eurofurence.App.Common.Results;
using Eurofurence.App.Common.Utility;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Fursuits;
using Eurofurence.App.Domain.Model.Fursuits.CollectingGame;
using Eurofurence.App.Domain.Model.Security;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Fursuits;
using Eurofurence.App.Server.Services.Abstractions.Telegram;
using Microsoft.Extensions.Logging;

namespace Eurofurence.App.Server.Services.Fursuits
{
    public class CollectingGameService:  ICollectingGameService
    {
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private readonly CollectionGameConfiguration _collectionGameConfiguration;
        private readonly IEntityRepository<FursuitBadgeRecord> _fursuitBadgeRepository;
        private readonly IEntityRepository<FursuitParticipationRecord> _fursuitParticipationRepository;
        private readonly IEntityRepository<PlayerParticipationRecord> _playerParticipationRepository;
        private readonly IEntityRepository<RegSysIdentityRecord> _regSysIdentityRepository;
        private readonly IEntityRepository<TokenRecord> _tokenRepository;
        private readonly ITelegramMessageSender _telegramMessageSender;
        private ILogger _logger;

        public CollectingGameService(
            ILoggerFactory loggerFactory,
            CollectionGameConfiguration collectionGameConfiguration,
            IEntityRepository<FursuitBadgeRecord> fursuitBadgeRepository,
            IEntityRepository<FursuitParticipationRecord> fursuitParticipationRepository,
            IEntityRepository<PlayerParticipationRecord> playerParticipationRepository,
            IEntityRepository<RegSysIdentityRecord> regSysIdentityRepository,
            IEntityRepository<TokenRecord> tokenRepository,
            ITelegramMessageSender telegramMessageSender
            )
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _collectionGameConfiguration = collectionGameConfiguration;
            _fursuitBadgeRepository = fursuitBadgeRepository;
            _fursuitParticipationRepository = fursuitParticipationRepository;
            _playerParticipationRepository = playerParticipationRepository;
            _regSysIdentityRepository = regSysIdentityRepository;
            _tokenRepository = tokenRepository;
            _telegramMessageSender = telegramMessageSender;
        }


        public async Task<FursuitParticipationInfo[]> GetFursuitParticipationInfoForOwnerAsync(string ownerUid)
        {
            using (new TimeTrap(time => _logger.LogTrace(
                LogEvents.CollectionGame,
                "Benchmark: GetFursuitParticipationInfoForOwnerAsync({ownerUid}): {time} ms",
                ownerUid, time.TotalMilliseconds)))
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
        }

        public async Task<PlayerParticipationInfo> GetPlayerParticipationInfoForPlayerAsync(string playerUid, string playerName)
        {
            using (new TimeTrap(time => _logger.LogTrace(
                LogEvents.CollectionGame,
                "Benchmark: GetPlayerParticipationInfoForPlayerAsync({playerUid}): {time} ms",
                playerUid, time.TotalMilliseconds)))
            {
                var response = new PlayerParticipationInfo() {Name = playerName};
                var playerParticipation =
                    await _playerParticipationRepository.FindOneAsync(a => a.PlayerUid == playerUid);

                if (playerParticipation == null) return response;

                response.CollectionCount = playerParticipation.CollectionCount;
                response.IsBanned = playerParticipation.IsBanned;

                var recentlyCollected = playerParticipation.CollectionEntries
                    .OrderByDescending(a => a.EventDateTimeUtc)
                    .Take(5)
                    .ToList();

                var recentlyCollectedFursuitParticipations =
                    (await _fursuitParticipationRepository.FindAllAsync(
                        recentlyCollected.Select(a => a.FursuitParticipationUid)))
                    .ToList();

                var recentlyCollectedFursuitBadges =
                    (await _fursuitBadgeRepository.FindAllAsync(
                        recentlyCollectedFursuitParticipations.Select(a => a.FursuitBadgeId)))
                    .ToList();

                response.RecentlyCollected = recentlyCollected.Select(a =>
                    {
                        var fursuitParticipation =
                            recentlyCollectedFursuitParticipations.Single(b => b.Id == a.FursuitParticipationUid);
                        var fursuitBadge =
                            recentlyCollectedFursuitBadges.Single(b => b.Id == fursuitParticipation.FursuitBadgeId);

                        return new PlayerParticipationInfo.BadgeInfo()
                        {
                            Id = fursuitBadge.Id,
                            Name = fursuitBadge.Name
                        };
                    })
                    .ToList();

                var playersAhead = await _playerParticipationRepository.FindAllAsync(
                    a => !a.IsBanned && a.PlayerUid != playerParticipation.PlayerUid
                         && a.CollectionCount >= playerParticipation.CollectionCount);

                response.ScoreboardRank =
                    playersAhead.Count(a => a.CollectionCount > playerParticipation.CollectionCount
                                            || (
                                                a.CollectionCount > 0 &&
                                                a.CollectionCount == playerParticipation.CollectionCount &&
                                                a.CollectionEntries.Max(b => b.EventDateTimeUtc) <
                                                playerParticipation.CollectionEntries.Max(b => b.EventDateTimeUtc)
                                            )) + 1;

                return response;
            }
        }

        public async Task<PlayerCollectionEntry[]> GetPlayerCollectionEntriesForPlayerAsync(string playerUid)
        {
            using (new TimeTrap(time => _logger.LogTrace(
                LogEvents.CollectionGame,
                "Benchmark: GetPlayerCollectionEntriesForPlayerAsync({playerUid}): {time} ms",
                playerUid, time.TotalMilliseconds)))
            {
                var playerParticipation =
                    await _playerParticipationRepository.FindOneAsync(a => a.PlayerUid == playerUid);
                if (playerParticipation == null) return new PlayerCollectionEntry[0];

                var collectionEntries = playerParticipation.CollectionEntries
                    .OrderByDescending(a => a.EventDateTimeUtc)
                    .ToList();

                var fursuitParticipations =
                    (await _fursuitParticipationRepository.FindAllAsync(
                        collectionEntries.Select(a => a.FursuitParticipationUid)))
                    .ToList();

                var fursuitBadges =
                    (await _fursuitBadgeRepository.FindAllAsync(
                        fursuitParticipations.Select(a => a.FursuitBadgeId)))
                    .ToList();

                return collectionEntries.Select(a =>
                    {
                        var fursuitParticipation =
                            fursuitParticipations.Single(b => b.Id == a.FursuitParticipationUid);
                        var fursuitBadge =
                            fursuitBadges.Single(b => b.Id == fursuitParticipation.FursuitBadgeId);

                        return new PlayerCollectionEntry()
                        {
                            BadgeId = fursuitBadge.Id,
                            Name = fursuitBadge.Name,
                            Species = fursuitBadge.Species,
                            Gender = fursuitBadge.Gender,
                            CollectionCount = fursuitParticipation.CollectionCount,
                            CollectedAtDateTimeUtc = a.EventDateTimeUtc
                        };
                    })
                    .OrderByDescending(a => a.CollectedAtDateTimeUtc)
                    .ToArray();
            }
        }

        public async Task<IResult> RegisterTokenForFursuitBadgeForOwnerAsync(string ownerUid, Guid fursuitBadgeId, string tokenValue)
        {
            using (new TimeTrap(time => _logger.LogTrace(
                LogEvents.CollectionGame,
                "Benchmark: RegisterTokenForFursuitBadgeForOwnerAsync({ownerUid}, {fursuitBadgeId},  {tokenValue}): {time} ms",
                ownerUid, fursuitBadgeId, tokenValue, time.TotalMilliseconds)))
            {
                try
                {
                    await _semaphore.WaitAsync();

                    var fursuitBadge = await _fursuitBadgeRepository.FindOneAsync(
                        badge => badge.OwnerUid == ownerUid && badge.Id == fursuitBadgeId);
                    if (fursuitBadge == null)
                    {
                        _logger.LogDebug(LogEvents.CollectionGame, "Failed RegisterTokenForFursuitBadgeForOwnerAsync for {ownerUid}: {reason}", ownerUid, "INVALID_FURSUIT_BADGE_ID");
                        return Result.Error("INVALID_FURSUIT_BADGE_ID", "Invalid fursuitBadgeId");
                    }

                    var token = await _tokenRepository.FindOneAsync(t => t.Value == tokenValue && t.IsLinked == false);
                    if (token == null)
                    {
                        _logger.LogDebug(LogEvents.CollectionGame, "Failed RegisterTokenForFursuitBadgeForOwnerAsync for {ownerUid}: {reason}", ownerUid, "INVALID_TOKEN");
                        return Result.Error("INVALID_TOKEN", "Invalid token");
                    }

                    var existingParticipation =
                        await _fursuitParticipationRepository.FindOneAsync(p => p.FursuitBadgeId == fursuitBadgeId);
                    if (existingParticipation != null)
                    {
                        _logger.LogDebug(LogEvents.CollectionGame, "Failed RegisterTokenForFursuitBadgeForOwnerAsync for {ownerUid}: {reason}", ownerUid, "EXISTING_PARTICIPATION");
                        return Result.Error("EXISTING_PARTICIPATION", "Fursuit already has a token assigned to it");
                    }

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

                    _logger.LogInformation(
                        LogEvents.CollectionGame,
                        "Successful RegisterTokenForFursuitBadgeForOwnerAsync for {ownerUid}: {fursuitName} now has token {tokenValue}",
                        ownerUid, fursuitBadge.Name, token.Value);

                    return Result.Ok;
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }

        public async Task<IResult<CollectTokenResponse>> CollectTokenForPlayerAsync(string playerUid, string tokenValue)
        {
            using (new TimeTrap(time => _logger.LogTrace(
                LogEvents.CollectionGame,
                "Benchmark: CollectTokenForPlayerAsync({playerUid}, {tokenValue}): {time} ms",
                    playerUid, tokenValue, time.TotalMilliseconds)))
            {
                if (string.IsNullOrWhiteSpace(tokenValue))
                {
                    _logger.LogTrace(LogEvents.CollectionGame, "Rejected CollectTokenForPlayerAsync (empty token) for player {playerUid}", playerUid);
                    return Result<CollectTokenResponse>.Error("EMPTY_TOKEN", "Token cannot be empty.");
                }

                try
                {
                    await _semaphore.WaitAsync();

                    var playerParticipation =
                        await _playerParticipationRepository.FindOneAsync(a => a.PlayerUid == playerUid);
                    if (playerParticipation == null)
                    {
                        playerParticipation = new PlayerParticipationRecord()
                        {
                            Id = Guid.NewGuid(),
                            PlayerUid = playerUid,
                            Karma = 0
                        };

                        _logger.LogDebug(LogEvents.CollectionGame, "Creating initial PlayerParticipationRecord for {playerUid}", playerUid);
                        await _playerParticipationRepository.InsertOneAsync(playerParticipation);
                    }

                    if (playerParticipation.IsBanned)
                    {
                        _logger.LogDebug(LogEvents.CollectionGame, "Rejected CollectTokenForPlayerAsync for banned player {playerUid}", playerUid);
                        return Result<CollectTokenResponse>.Error("BANNED",
                            "You have been disqualified from the game.");
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
                                _logger.LogInformation(LogEvents.CollectionGame, "Failed CollectTokenForPlayerAsync for {playerUid} using token {tokenValue}: {reason}", playerUid, tokenValue, "INVALID_TOKEN_BAN_IMMINENT");
                                return Result<CollectTokenResponse>.Error("INVALID_TOKEN_BAN_IMMINENT", sb.ToString());
                            }
                            else
                            {
                                var identity =
                                    await _regSysIdentityRepository.FindOneAsync(a => a.Uid == playerParticipation.PlayerUid);

                                await SendToTelegramManagementChannelAsync(
                                    $"*Player Banned:*\n{playerParticipation.PlayerUid} ({identity.Username})\n(Had {playerParticipation.CollectionCount} codes successfully collected so far.)");

                                sb.Append(" You have been disqualified.");
                                _logger.LogWarning(LogEvents.CollectionGame, "Failed CollectTokenForPlayerAsync for {playerUid} using token {tokenValue}: {reason}", playerUid, tokenValue, "INVALID_TOKEN_BANNED");
                                return Result<CollectTokenResponse>.Error("INVALID_TOKEN_BANNED", sb.ToString());
                            }
                        }

                        _logger.LogDebug(LogEvents.CollectionGame, "Failed CollectTokenForPlayerAsync for {playerUid} using token {tokenValue}: {reason}", playerUid, tokenValue, "INVALID_TOKEN");
                        return Result<CollectTokenResponse>.Error("INVALID_TOKEN", sb.ToString());
                    }

                    if (playerParticipation.PlayerUid == fursuitParticipation.OwnerUid)
                    {
                        _logger.LogDebug(LogEvents.CollectionGame, "Failed CollectTokenForPlayerAsync for {playerUid} using token {tokenValue}: {reason}", playerUid, tokenValue, "INVALID_TOKEN_OWN_SUIT");
                        return Result<CollectTokenResponse>.Error("INVALID_TOKEN_OWN_SUIT",
                            "You cannot collect your own fursuits.");
                    }

                    if (playerParticipation.CollectionEntries.Any(
                        a => a.FursuitParticipationUid == fursuitParticipation.Id))
                    {
                        _logger.LogDebug(LogEvents.CollectionGame, "Failed CollectTokenForPlayerAsync for {playerUid} using token {tokenValue}: {reason}", playerUid, tokenValue, "INVALID_TOKEN_ALREADY_COLLECTED");
                        return Result<CollectTokenResponse>.Error("INVALID_TOKEN_ALREADY_COLLECTED",
                            "You have already collected this suit!");
                    }

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

                    _logger.LogInformation(
                        LogEvents.CollectionGame,
                        "Successful CollectTokenForPlayerAsync for {playerUid} using token {tokenValue} (now has {playerCollectionCount} catches): {fursuitName} ({fursuitCollectionCount} times caught)",
                        playerUid, tokenValue, playerParticipation.CollectionCount, fursuitBadge.Name,
                        fursuitParticipation.CollectionCount);

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
    }

        public async Task<IResult<PlayerScoreboardEntry[]>> GetPlayerScoreboardEntriesAsync(int top)
        {
            using (new TimeTrap(time => _logger.LogTrace(
                LogEvents.CollectionGame, 
                "Benchmark: GetPlayerScoreboardEntriesAsync({top}): {time} ms",
                top, time.TotalMilliseconds)))
            {
                var topPlayers = (await _playerParticipationRepository
                        .FindAllAsync(a => !a.IsBanned && a.IsDeleted == 0 && a.CollectionCount > 0))
                    .OrderByDescending(a => a.CollectionCount)
                    .ThenBy(a => a.CollectionEntries.Max(b => b.EventDateTimeUtc))
                    .Take(top)
                    .ToList();

                var topPlayersUids = topPlayers.Select(a => a.PlayerUid);
                var identities = await _regSysIdentityRepository.FindAllAsync(a => topPlayersUids.Contains(a.Uid));

                var result = topPlayers
                    .Select(a => new PlayerScoreboardEntry()
                    {
                        Name = identities.SingleOrDefault(b => b.Uid == a.PlayerUid)?.Username ?? "(?)",
                        CollectionCount = a.CollectionCount,
                    })
                    .ToArray();

                for (var i = 0; i < result.Length; i++) result[i].Rank = i + 1;
                return Result<PlayerScoreboardEntry[]>.Ok(result);
            }
        }

        public async Task<IResult<FursuitScoreboardEntry[]>> GetFursuitScoreboardEntriesAsync(int top)
        {
            using (new TimeTrap(time => _logger.LogTrace(
                LogEvents.CollectionGame, 
                "Benchmark: GetFursuitScoreboardEntriesAsync({top}): {time} ms",
                top, time.TotalMilliseconds)))
            {
                var topFursuits = (await _fursuitParticipationRepository
                        .FindAllAsync(a => !a.IsBanned && a.IsDeleted == 0 && a.CollectionCount > 0))
                    .OrderByDescending(a => a.CollectionCount)
                    .ThenBy(a => a.CollectionEntries.Max(b => b.EventDateTimeUtc))
                    .Take(top)
                    .ToList();

                var fursuitBadges =
                    await _fursuitBadgeRepository.FindAllAsync(topFursuits.Select(a => a.FursuitBadgeId));

                var result = topFursuits
                    .Select(entry =>
                    {
                        var fursuitBadge = fursuitBadges.Single(badge => badge.Id == entry.FursuitBadgeId);
                        return new FursuitScoreboardEntry()
                        {
                            Name = fursuitBadge.Name,
                            CollectionCount = entry.CollectionCount,
                            Gender = fursuitBadge.Gender,
                            Species = fursuitBadge.Species,
                            BadgeId = fursuitBadge.Id
                        };
                    })
                    .ToArray();

                for (var i = 0; i < result.Length; i++) result[i].Rank = i + 1;
                return Result<FursuitScoreboardEntry[]>.Ok(result);
            }
        }

        public async Task<IResult> CreateTokenFromValueAsync(string tokenValue)
        {
            if (string.IsNullOrWhiteSpace(tokenValue))
                return Result.Error("TOKEN_EMPTY", "Token cannot be empty");

            var tokenRecord = new TokenRecord
            {
                Id = Guid.NewGuid(),
                LastChangeDateTimeUtc = DateTime.UtcNow,
                Value = tokenValue
            };

            try
            {
                await _semaphore.WaitAsync();
                await _tokenRepository.InsertOneAsync(tokenRecord);

                return Result.Ok;
            }
            catch (Exception ex)
            {
                return Result.Error("EXCEPTION", ex.Message);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<IResult> CreateTokensFromValuesAsync(string[] tokenValues)
        {
            foreach (var tokenValue in tokenValues)
            {
                var result = await CreateTokenFromValueAsync(tokenValue);
                if (!result.IsSuccessful) return result;
            }

            return Result.Ok;
        }

        public async Task<IResult> UnbanPlayerAsync(string playerUid)
        {
            using (new TimeTrap(time => _logger.LogTrace(
                LogEvents.CollectionGame, 
                "Benchmark: RegisterTokenForFursuitBadgeForOwnerAsync({playerUid}): {time} ms",
                playerUid, time.TotalMilliseconds)))
            {
                try
                {
                    await _semaphore.WaitAsync();

                    var playerParticipation =
                        await _playerParticipationRepository.FindOneAsync(a => a.PlayerUid == playerUid);

                    if (playerParticipation == null)
                        return Result.Error("INVALID_PLAYERUID", $"No player found with uid = {playerUid}");

                    if (playerParticipation.IsBanned == false)
                        return Result.Error("NOT_BANNED", $"Player with uid {playerUid} is not banned.");

                    playerParticipation.IsBanned = false;
                    playerParticipation.Karma = 0;
                    playerParticipation.Touch();

                    await _playerParticipationRepository.ReplaceOneAsync(playerParticipation);
                    await SendToTelegramManagementChannelAsync($"*Player Unbanned*: {playerParticipation.PlayerUid}");

                    return Result.Ok;
                }
                finally
                {
                    _semaphore.Release();
                }
            }
        }

        private async Task SendToTelegramManagementChannelAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(_collectionGameConfiguration.TelegramManagementChatId)) return;

            await _telegramMessageSender.SendMarkdownMessageToChatAsync(_collectionGameConfiguration.TelegramManagementChatId, message);
        }
    }
}
