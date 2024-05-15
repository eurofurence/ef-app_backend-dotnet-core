using Eurofurence.App.Common.Results;
using Eurofurence.App.Domain.Model.Fursuits;
using Eurofurence.App.Domain.Model.Fursuits.CollectingGame;
using Eurofurence.App.Domain.Model.Security;
using Eurofurence.App.Server.Services.Abstractions.Fursuits;
using Eurofurence.App.Server.Services.Abstractions.Telegram;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Fursuits;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Eurofurence.App.Server.Services.Tests
{
    public class CollectionGameServiceTests
    {
        private readonly TestDbContextFactory _testDbContextFactory;
        private const string INVALID_TOKEN = "INVALID-TOKEN";

        public CollectionGameServiceTests()
        {
            _testDbContextFactory = new TestDbContextFactory();
        }

        private AppDbContext GetCleanDbContext()
        {
            var context = _testDbContextFactory.CreateDbContext();
            context.Database.EnsureDeleted();
            return context;
        }

        private CollectingGameService GetService(AppDbContext appDbContext)
        {
            return new CollectingGameService(appDbContext, 
                new NullLoggerFactory(), 
                new CollectionGameConfiguration(),
                new Mock<ITelegramMessageSender>().As<ITelegramMessageSender>().Object);
        }

        private async Task<RegSysIdentityRecord> CreateRegSysIdentityAsync(AppDbContext appDbContext)
        {
            var id = Guid.NewGuid();

            var role = new RoleRecord()
            {
                Name = "Attendee"
            };
            appDbContext.Roles.Add(role);

            var record = new RegSysIdentityRecord()
            {
                Id = id,
                Uid = $"Test:{id}",
                Username = $"Test Attendee {id}",
                Roles = [role],
            };

            appDbContext.RegSysIdentities.Add(record);
            await appDbContext.SaveChangesAsync();
            return record;
        }

        private async Task<FursuitBadgeRecord> CreateFursuitBadgeAsync(AppDbContext appDbContext, string ownerUid)
        {
            var record = new FursuitBadgeRecord()
            {
                Id = Guid.NewGuid(),
                OwnerUid = ownerUid,
                Name = $"Suit of attendee {ownerUid}",
                Gender = "male",
                Species = "wolf",
                WornBy = "me"
            };

            appDbContext.FursuitBadges.Add(record);
            await appDbContext.SaveChangesAsync();
            return record;
        }

        private async Task<TokenRecord> CreateTokenAsync(AppDbContext appDbContext)
        {
            var id = Guid.NewGuid();
            var record = new TokenRecord()
            {
                Id = id,
                Value = id.ToString()
            };

            appDbContext.Tokens.Add(record);
            await appDbContext.SaveChangesAsync();
            return record;
        }

        [Fact(DisplayName = "Token Registration: Valid scenario")]
        public async Task CollectingGameService_WhenRegisteringValidTokenForAFursuitBadge_ThenOperationsSuccessful()
        {
            var appDbContext = GetCleanDbContext();
            var collectingGameService = GetService(appDbContext);

            var testUser = await CreateRegSysIdentityAsync(appDbContext);
            var testToken = await CreateTokenAsync(appDbContext);
            var testUserFursuitBadge = await CreateFursuitBadgeAsync(appDbContext, ownerUid: testUser.Uid);
            
            var result = await collectingGameService.RegisterTokenForFursuitBadgeForOwnerAsync(testUser.Uid, testUserFursuitBadge.Id, testToken.Value);
            var testUserFursuitParticipation = await appDbContext.FursuitParticipations.AsNoTracking().FirstOrDefaultAsync(a => a.OwnerUid == testUser.Uid);
            testToken = await appDbContext.Tokens.AsNoTracking().FirstOrDefaultAsync(entity => entity.Id == testToken.Id);

            Assert.True(result.IsSuccessful);
            Assert.NotNull(testUserFursuitParticipation);
            Assert.True(testToken.IsLinked);
            Assert.Equal(testToken.LinkedFursuitParticipantUid, testUserFursuitParticipation.Id);
        }

        [Fact(DisplayName = "Token Registration: Passing invalid token fails")]
        public async Task CollectingGameService_WhenRegisteringInvalidTokenForAFursuitBadge_ThenOperationFailsWithCodeInvalidToken()
        {
            var appDbContext = GetCleanDbContext();
            var collectingGameService = GetService(appDbContext);

            var testUser = await CreateRegSysIdentityAsync(appDbContext);
            var testToken = INVALID_TOKEN;
            var testUserFursuitBadge = await CreateFursuitBadgeAsync(appDbContext, ownerUid: testUser.Uid);
            
            Assert.NotNull(testUser);
            Assert.NotNull(testUserFursuitBadge);

            var result = await collectingGameService.RegisterTokenForFursuitBadgeForOwnerAsync(testUser.Uid, testUserFursuitBadge.Id, testToken);
            var testUserFursuitParticipation = await appDbContext.FursuitParticipations.AsNoTracking().FirstOrDefaultAsync(a => a.OwnerUid == testUser.Uid);

            Assert.False(result.IsSuccessful);
            Assert.Equal("INVALID_TOKEN", result.ErrorCode);
            Assert.Null(testUserFursuitParticipation);
        }

        [Fact(DisplayName = "Token Registration: Passing invalid fursuitBadgeId fails")]
        public async Task CollectingGameService_WhenRegisteringValidTokenForInvalidFursuitBadge_ThenOperationFailsWithCodeInvalidToken()
        {
            var appDbContext = GetCleanDbContext();
            var collectingGameService = GetService(appDbContext);

            var testUser = await CreateRegSysIdentityAsync(appDbContext);
            var testToken = await CreateTokenAsync(appDbContext);
            var invalidFursuitBadgeId = Guid.NewGuid();
            
            Assert.NotNull(testUser);
            Assert.NotNull(testToken);

            var result = await collectingGameService.RegisterTokenForFursuitBadgeForOwnerAsync(testUser.Uid, invalidFursuitBadgeId, testToken.Value);

            Assert.False(result.IsSuccessful);
            Assert.Equal("INVALID_FURSUIT_BADGE_ID", result.ErrorCode);
        }


        private struct TwoPlayersWithOneFursuitToken
        {
            public RegSysIdentityRecord player1WithFursuit;
            public FursuitBadgeRecord player1FursuitBadge;
            public TokenRecord player1Token;
            public RegSysIdentityRecord player2WithoutFursuit;
        }

        private async Task<TwoPlayersWithOneFursuitToken> SetupTwoPlayersWithOneFursuitTokenAsync()
        {
            var appDbContext = GetCleanDbContext();
            var collectingGameService = GetService(appDbContext);

            var result = new TwoPlayersWithOneFursuitToken();

            result.player1WithFursuit = await CreateRegSysIdentityAsync(appDbContext);
            result.player1Token = await CreateTokenAsync(appDbContext);
            result.player1FursuitBadge = await CreateFursuitBadgeAsync(appDbContext, result.player1WithFursuit.Uid);

            await collectingGameService.RegisterTokenForFursuitBadgeForOwnerAsync(
                result.player1WithFursuit.Uid,
                result.player1FursuitBadge.Id,
                result.player1Token.Value
            );

            result.player2WithoutFursuit = await CreateRegSysIdentityAsync(appDbContext);

            return result;
        }

        [Fact(DisplayName = "Collect Token: Valid scenario")]
        public async Task CollectingGameService_WhenCollectingValidToken_ThenOperationIsSuccessful()
        {
            var appDbContext = GetCleanDbContext();
            var collectingGameService = GetService(appDbContext);

            var setup = await SetupTwoPlayersWithOneFursuitTokenAsync();

            var result = await collectingGameService.CollectTokenForPlayerAsync(
                setup.player2WithoutFursuit.Uid, 
                setup.player1Token.Value
            );

            var player1FursuitParticipation = await appDbContext.FursuitParticipations
                .AsNoTracking()
                .Include(fursuitParticipationRecord => fursuitParticipationRecord.CollectionEntries).FirstOrDefaultAsync(a => a.OwnerUid == setup.player1WithFursuit.Uid);
            var player2PlayerParticipation = await appDbContext.PlayerParticipations
                .AsNoTracking()
                .Include(playerParticipationRecord => playerParticipationRecord.CollectionEntries).FirstOrDefaultAsync(a => a.PlayerUid == setup.player2WithoutFursuit.Uid);

            Assert.True(result.IsSuccessful);
            Assert.Equal(1, player1FursuitParticipation.CollectionCount);
            Assert.Equal(1, player2PlayerParticipation.CollectionCount);
            Assert.Contains(player1FursuitParticipation.CollectionEntries, a => a.PlayerParticipationId == setup.player2WithoutFursuit.Uid);
            Assert.Contains(player2PlayerParticipation.CollectionEntries, a => a.FursuitParticipationId == player1FursuitParticipation.Id);
        }

        [Fact(DisplayName = "Collect Token: Collecting valid token twice fails")]
        public async Task CollectingGameService_WhenCollectingValidTokenMultipleTimes_ThenOperationFails()
        {
            var appDbContext = GetCleanDbContext();
            var collectingGameService = GetService(appDbContext);

            var setup = await SetupTwoPlayersWithOneFursuitTokenAsync();

            await collectingGameService.CollectTokenForPlayerAsync(
                setup.player2WithoutFursuit.Uid, 
                setup.player1Token.Value
            );

            var result = await collectingGameService.CollectTokenForPlayerAsync(
                setup.player2WithoutFursuit.Uid, 
                setup.player1Token.Value
            );

            Assert.False(result.IsSuccessful);
            Assert.Equal("INVALID_TOKEN_ALREADY_COLLECTED", result.ErrorCode);
        }


        [Fact(DisplayName = "Collect Token: Collecting an invalid token fails")]
        public async Task CollectingGameService_WhenCollectingInvalidToken_ThenOperationFails()
        {
            var appDbContext = GetCleanDbContext();
            var collectingGameService = GetService(appDbContext);

            var setup = await SetupTwoPlayersWithOneFursuitTokenAsync();

            var result = await collectingGameService.CollectTokenForPlayerAsync(
                setup.player2WithoutFursuit.Uid, 
                INVALID_TOKEN
            );

            Assert.False(result.IsSuccessful);
            Assert.Equal("INVALID_TOKEN", result.ErrorCode);
        }

        [Fact(DisplayName = "Collect Token: Collecting your own suit fails")]
        public async Task CollectingGameService_WhenCollectingValidTokenOfYourOwnSuit_ThenOperationFails()
        {
            var appDbContext = GetCleanDbContext();
            var collectingGameService = GetService(appDbContext);

            var setup = await SetupTwoPlayersWithOneFursuitTokenAsync();

            var result = await collectingGameService.CollectTokenForPlayerAsync(
                setup.player1WithFursuit.Uid,
                setup.player1Token.Value
            );

            Assert.False(result.IsSuccessful);
            Assert.Equal("INVALID_TOKEN_OWN_SUIT", result.ErrorCode);
        }

        [Fact(DisplayName = "Collect Token: Collecting too many wrong tokens leads to a ban")]
        public async Task CollectingGameService_WhenCollectingManyInvalidTokenOfYourOwnSuit_ThenPlayerIsBanned()
        {
            var appDbContext = GetCleanDbContext();
            var collectingGameService = GetService(appDbContext);

            var setup = await SetupTwoPlayersWithOneFursuitTokenAsync();

            IResult<CollectTokenResponse> result = null;
            for (int i = 0; i < 20; i++)
                result = await collectingGameService.CollectTokenForPlayerAsync(
                    setup.player1WithFursuit.Uid, 
                    INVALID_TOKEN
                );

            Assert.False(result.IsSuccessful);
            Assert.Equal("BANNED", result.ErrorCode);
        }

        [Fact(DisplayName = "Scoreboard: For players/fursuits with identical collection count, the earlier achiever ranks higher")]
        public async Task CollectingGameService_WhenPlayersOrFursuitsHaveSameCollectionCount_ThenWhoeverAchievedItFirstRanksHigher()
        {
            var appDbContext = GetCleanDbContext();
            var collectingGameService = GetService(appDbContext);

            var players = new RegSysIdentityRecord[3];
            var tokens = new TokenRecord[3];
            var fursuitBadges = new FursuitBadgeRecord[3];

            // 3 players, each with suit
            for (int i = 0; i < 3; i++)
            {
                players[i] = await CreateRegSysIdentityAsync(appDbContext);
                tokens[i] = await CreateTokenAsync(appDbContext);
                fursuitBadges[i] = await CreateFursuitBadgeAsync(appDbContext, players[i].Uid);

                await collectingGameService.RegisterTokenForFursuitBadgeForOwnerAsync(
                    players[i].Uid, fursuitBadges[i].Id, tokens[i].Value);
            }

            // Each catches everyone else.
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (j == i) continue;

                    var result = await collectingGameService.CollectTokenForPlayerAsync(
                        players[i].Uid, tokens[j].Value);

                    Assert.True(result.IsSuccessful);
                }
            }

            var playerScoreboard = await collectingGameService.GetPlayerScoreboardEntriesAsync(5);
            var fursuitScoreboard = await collectingGameService.GetFursuitScoreboardEntriesAsync(5);

            // 1 catches first, then 2, then 3
            Assert.Equal(3, playerScoreboard.Value.Length);
            Assert.True(playerScoreboard.Value.All(a => a.CollectionCount == 2));
            Assert.Equal(players[0].Username, playerScoreboard.Value[0].Name);
            Assert.Equal(players[1].Username, playerScoreboard.Value[1].Name);
            Assert.Equal(players[2].Username, playerScoreboard.Value[2].Name);

            // 3 gets the 2 catches first (by 1 + 2), then 1 (by 2 + 3), then 2
            Assert.Equal(3, fursuitScoreboard.Value.Length);
            Assert.True(fursuitScoreboard.Value.All(a => a.CollectionCount == 2));
            Assert.Equal(fursuitBadges[2].Id, fursuitScoreboard.Value[0].BadgeId);
            Assert.Equal(fursuitBadges[0].Id, fursuitScoreboard.Value[1].BadgeId);
            Assert.Equal(fursuitBadges[1].Id, fursuitScoreboard.Value[2].BadgeId);
        }
    }
}
