using Autofac;
using Eurofurence.App.Common.Results;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Fursuits;
using Eurofurence.App.Domain.Model.Fursuits.CollectingGame;
using Eurofurence.App.Domain.Model.Security;
using Eurofurence.App.Server.Services.Abstractions.Fursuits;
using Eurofurence.App.Server.Services.Abstractions.Telegram;
using Eurofurence.App.Server.Services.Fursuits;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Eurofurence.App.Server.Services.Tests
{
    public class CollectionGameServiceTests
    {
        protected IContainer _container;
        private readonly IEntityRepository<FursuitBadgeRecord> _fursuitBadgeRepository;
        private readonly IEntityRepository<FursuitParticipationRecord> _fursuitParticipationRepository;
        private readonly IEntityRepository<PlayerParticipationRecord> _playerParticipationRepository;
        private readonly IEntityRepository<RegSysIdentityRecord> _regSysIdentityRepository;
        private readonly IEntityRepository<TokenRecord> _tokenRepository;

        private const string INVALID_TOKEN = "INVALID-TOKEN";

        public CollectionGameServiceTests()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new App.Tests.Common.TestLogger.AutofacModule());
            builder.RegisterModule(new App.Tests.Common.InMemoryRepository.AutofacModule());
            builder.RegisterModule(new DependencyResolution.AutofacModule());

            var _telegramMessageSender = new Mock<ITelegramMessageSender>().As<ITelegramMessageSender>();
            var _configuration = new CollectionGameConfiguration();

            builder.RegisterInstance(_telegramMessageSender.Object).As<ITelegramMessageSender>();
            builder.RegisterInstance(_configuration).As<CollectionGameConfiguration>();

            _container = builder.Build();

            _fursuitParticipationRepository = _container.Resolve<IEntityRepository<FursuitParticipationRecord>>();
            _playerParticipationRepository = _container.Resolve<IEntityRepository<PlayerParticipationRecord>>();
            _regSysIdentityRepository = _container.Resolve<IEntityRepository<RegSysIdentityRecord>>();
            _fursuitBadgeRepository = _container.Resolve<IEntityRepository<FursuitBadgeRecord>>();
            _tokenRepository = _container.Resolve<IEntityRepository<TokenRecord>>();

            PopulateDemoData(_container).Wait();
        }

        private async Task PopulateDemoData(IContainer container)
        {

            for (int i = 0; i < 20; i++)
            {
                await _regSysIdentityRepository.InsertOneAsync(new RegSysIdentityRecord()
                {
                    Id = Guid.NewGuid(),
                    Uid = $"Test:{i}",
                    Username = "$Test Attendee {i}",
                    Roles = new[] { "Attendee" },
                });
            }


            for (int i = 0; i < 20; i++)
            {
                await _fursuitBadgeRepository.InsertOneAsync(new FursuitBadgeRecord()
                {
                    Id = Guid.NewGuid(),
                    OwnerUid = $"Test:{i}",
                    Name = "Suit of attendee {i}"
                });
            }


            for (int i = 0; i < 20; i++)
            {
                await _tokenRepository.InsertOneAsync(new TokenRecord()
                {
                    Id = Guid.NewGuid(),
                    Value = $"TOKEN-{i}"
                });
            }
        }


        [Fact(DisplayName = "Token Registration: Valid scenario")]
        public async Task CollectingGameService_WhenRegisteringValidTokenForAFursuitBadge_ThenOperationsSuccessful()
        {
            var cgs = _container.Resolve<ICollectingGameService>();

            var testUser = (await _regSysIdentityRepository.FindAllAsync()).First();
            var testToken = (await _tokenRepository.FindAllAsync()).First();
            var testUserFursuitBadge = await _fursuitBadgeRepository.FindOneAsync(a => a.OwnerUid == testUser.Uid);
            var testUserFursuitParticipation = await _fursuitParticipationRepository.FindOneAsync(a => a.OwnerUid == testUser.Uid);

            Assert.NotNull(testUser);
            Assert.NotNull(testUserFursuitBadge);
            Assert.Null(testUserFursuitParticipation);
            Assert.NotNull(testToken);
            Assert.False(testToken.IsLinked);

            var result = await cgs.RegisterTokenForFursuitBadgeForOwnerAsync(testUser.Uid, testUserFursuitBadge.Id, testToken.Value);
            testUserFursuitParticipation = await _fursuitParticipationRepository.FindOneAsync(a => a.OwnerUid == testUser.Uid);
            testToken = await _tokenRepository.FindOneAsync(testToken.Id);

            Assert.True(result.IsSuccessful);
            Assert.NotNull(testUserFursuitParticipation);
            Assert.True(testToken.IsLinked);
            Assert.Equal(testToken.LinkedFursuitParticipantUid, testUserFursuitParticipation.Id);
        }

        [Fact(DisplayName = "Token Registration: Passing invalid token fails")]
        public async Task CollectingGameService_WhenRegisteringInvalidTokenForAFursuitBadge_ThenOperationFailsWithCodeInvalidToken()
        {
            var cgs = _container.Resolve<ICollectingGameService>();

            var testUser = (await _regSysIdentityRepository.FindAllAsync()).First();
            var testToken = INVALID_TOKEN;
            var testUserFursuitBadge = await _fursuitBadgeRepository.FindOneAsync(a => a.OwnerUid == testUser.Uid);
            var testUserFursuitParticipation = await _fursuitParticipationRepository.FindOneAsync(a => a.OwnerUid == testUser.Uid);

            Assert.NotNull(testUser);
            Assert.NotNull(testUserFursuitBadge);
            Assert.Null(testUserFursuitParticipation);

            var result = await cgs.RegisterTokenForFursuitBadgeForOwnerAsync(testUser.Uid, testUserFursuitBadge.Id, testToken);
            testUserFursuitParticipation = await _fursuitParticipationRepository.FindOneAsync(a => a.OwnerUid == testUser.Uid);

            Assert.False(result.IsSuccessful);
            Assert.Equal("INVALID_TOKEN", result.ErrorCode);
            Assert.Null(testUserFursuitParticipation);
        }

        [Fact(DisplayName = "Token Registration: Passing invalid fursuitBadgeId fails")]
        public async Task CollectingGameService_WhenRegisteringValidTokenForInvalidFursuitBadge_ThenOperationFailsWithCodeInvalidToken()
        {
            var cgs = _container.Resolve<ICollectingGameService>();

            var testUser = (await _regSysIdentityRepository.FindAllAsync()).First();
            var testToken = (await _tokenRepository.FindAllAsync()).First();
            var invalidFursuitBadgeId = Guid.NewGuid();

            Assert.NotNull(testUser);
            Assert.NotNull(testToken);

            var result = await cgs.RegisterTokenForFursuitBadgeForOwnerAsync(testUser.Uid, invalidFursuitBadgeId, testToken.Value);

            Assert.False(result.IsSuccessful);
            Assert.Equal("INVALID_FURSUIT_BADGE_ID", result.ErrorCode);
        }


        public struct TwoPlayersWithOneFursuitToken
        {
            public RegSysIdentityRecord player1WithFursuit;
            public FursuitBadgeRecord player1FursuitBadge;
            public TokenRecord player1Token;
            public RegSysIdentityRecord player2WithoutFursuit;
        }

        private async Task<TwoPlayersWithOneFursuitToken> SetupTwoPlayersWithOneFursuitTokenAsync()
        {
            var cgs = _container.Resolve<ICollectingGameService>();
            var result = new TwoPlayersWithOneFursuitToken();

            var playerPool = (await _regSysIdentityRepository.FindAllAsync()).Take(2).ToList();

            result.player1WithFursuit = playerPool[0];
            result.player1Token = (await _tokenRepository.FindAllAsync()).First();
            result.player1FursuitBadge = await _fursuitBadgeRepository.FindOneAsync(a => a.OwnerUid == result.player1WithFursuit.Uid);

            await cgs.RegisterTokenForFursuitBadgeForOwnerAsync(result.player1WithFursuit.Uid, result.player1FursuitBadge.Id, result.player1Token.Value);

            result.player2WithoutFursuit = playerPool[1];

            return result;
        }

        [Fact(DisplayName = "Collect Token: Valid scenario")]
        public async Task CollectingGameService_WhenCollectingValidToken_ThenOperationIsSuccessful()
        {
            var cgs = _container.Resolve<ICollectingGameService>();
            var setup = await SetupTwoPlayersWithOneFursuitTokenAsync();

            var result = await cgs.CollectTokenForPlayerAsync(setup.player2WithoutFursuit.Uid, setup.player1Token.Value);

            var player1FursuitParticipation = await _fursuitParticipationRepository.FindOneAsync(a => a.OwnerUid == setup.player1WithFursuit.Uid);
            var player2PlayerParticipation = await _playerParticipationRepository.FindOneAsync(a => a.PlayerUid == setup.player2WithoutFursuit.Uid);

            Assert.True(result.IsSuccessful);
            Assert.Equal(1, player1FursuitParticipation.CollectionCount);
            Assert.Equal(1, player2PlayerParticipation.CollectionCount);
            Assert.Contains(player1FursuitParticipation.CollectionEntries, a => a.PlayerParticipationUid == setup.player2WithoutFursuit.Uid);
            Assert.Contains(player2PlayerParticipation.CollectionEntries, a => a.FursuitParticipationUid == player1FursuitParticipation.Id);
        }

        [Fact(DisplayName = "Collect Token: Collecting valid token twice fails")]
        public async Task CollectingGameService_WhenCollectingValidTokenMultipleTimes_ThenOperationFails()
        {
            var cgs = _container.Resolve<ICollectingGameService>();
            var setup = await SetupTwoPlayersWithOneFursuitTokenAsync();

            await cgs.CollectTokenForPlayerAsync(setup.player2WithoutFursuit.Uid, setup.player1Token.Value);
            var result = await cgs.CollectTokenForPlayerAsync(setup.player2WithoutFursuit.Uid, setup.player1Token.Value);

            Assert.False(result.IsSuccessful);
            Assert.Equal("INVALID_TOKEN_ALREADY_COLLECTED", result.ErrorCode);
        }


        [Fact(DisplayName = "Collect Token: Collecting an invalid token fails")]
        public async Task CollectingGameService_WhenCollectingInvalidToken_ThenOperationFails()
        {
            var cgs = _container.Resolve<ICollectingGameService>();
            var setup = await SetupTwoPlayersWithOneFursuitTokenAsync();

            var result = await cgs.CollectTokenForPlayerAsync(setup.player2WithoutFursuit.Uid, INVALID_TOKEN);

            Assert.False(result.IsSuccessful);
            Assert.Equal("INVALID_TOKEN", result.ErrorCode);
        }

        [Fact(DisplayName = "Collect Token: Collecting your own suit fails")]
        public async Task CollectingGameService_WhenCollectingValidTokenOfYourOwnSuit_ThenOperationFails()
        {
            var cgs = _container.Resolve<ICollectingGameService>();
            var setup = await SetupTwoPlayersWithOneFursuitTokenAsync();

            var result = await cgs.CollectTokenForPlayerAsync(setup.player1WithFursuit.Uid, setup.player1Token.Value);

            Assert.False(result.IsSuccessful);
            Assert.Equal("INVALID_TOKEN_OWN_SUIT", result.ErrorCode);
        }

        [Fact(DisplayName = "Collect Token: Collecting too many wrong tokens leads to a ban")]
        public async Task CollectingGameService_WhenCollectingManyInvalidTokenOfYourOwnSuit_ThenPlayerIsBanned()
        {
            var cgs = _container.Resolve<ICollectingGameService>();
            var setup = await SetupTwoPlayersWithOneFursuitTokenAsync();

            IResult<CollectTokenResponse> result = null;
            for (int i = 0; i < 20; i++)
                result = await cgs.CollectTokenForPlayerAsync(setup.player1WithFursuit.Uid, INVALID_TOKEN);

            Assert.False(result.IsSuccessful);
            Assert.Equal("BANNED", result.ErrorCode);
        }
    }
}
