using Autofac;
using Eurofurence.App.Server.Services.Abstraction.Telegram;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Announcements;
using Eurofurence.App.Server.Services.Abstractions.ArtistsAlley;
using Eurofurence.App.Server.Services.Abstractions.ArtShow;
using Eurofurence.App.Server.Services.Abstractions.Communication;
using Eurofurence.App.Server.Services.Abstractions.Dealers;
using Eurofurence.App.Server.Services.Abstractions.Events;
using Eurofurence.App.Server.Services.Abstractions.Fursuits;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Server.Services.Abstractions.Knowledge;
using Eurofurence.App.Server.Services.Abstractions.Maps;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Eurofurence.App.Server.Services.Abstractions.Telegram;
using Eurofurence.App.Server.Services.Abstractions.Validation;
using Eurofurence.App.Server.Services.Announcements;
using Eurofurence.App.Server.Services.ArtistsAlley;
using Eurofurence.App.Server.Services.ArtShow;
using Eurofurence.App.Server.Services.Communication;
using Eurofurence.App.Server.Services.Dealers;
using Eurofurence.App.Server.Services.Events;
using Eurofurence.App.Server.Services.Fursuits;
using Eurofurence.App.Server.Services.Images;
using Eurofurence.App.Server.Services.Knowledge;
using Eurofurence.App.Server.Services.Maps;
using Eurofurence.App.Server.Services.PushNotifications;
using Eurofurence.App.Server.Services.Security;
using Eurofurence.App.Server.Services.Storage;
using Eurofurence.App.Server.Services.Telegram;
using Eurofurence.App.Server.Services.Validation;
using Microsoft.Extensions.Configuration;
using System;

namespace Eurofurence.App.Server.Services.DependencyResolution
{
    public class AutofacModule : Module
    {
        private readonly IConfiguration _configuration;

        public AutofacModule(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            RegisterConfigurations(builder);
            RegisterServices(builder);
        }

        private void RegisterConfigurations(ContainerBuilder builder)
        {
            if (_configuration == null) return;

            builder.RegisterInstance(new ConventionSettings()
            {
                ConventionIdentifier = _configuration["global:conventionIdentifier"],
                IsRegSysAuthenticationEnabled = Convert.ToInt32(_configuration["global:regSysAuthenticationEnabled"]) == 1,
                ApiBaseUrl = _configuration["global:apiBaseUrl"]
            });

            builder.RegisterInstance(new TokenFactorySettings
            {
                SecretKey = _configuration["oAuth:secretKey"],
                Audience = _configuration["oAuth:audience"],
                Issuer = _configuration["oAuth:issuer"]
            });

            builder.RegisterInstance(new AuthenticationSettings
            {
                DefaultTokenLifeTime = TimeSpan.FromDays(30)
            });

            builder.RegisterInstance(new WnsConfiguration
            {
                ClientId = _configuration["wns:clientId"],
                ClientSecret = _configuration["wns:clientSecret"],
                TargetTopic = _configuration["wns:targetTopic"]
            });

            builder.RegisterInstance(new FirebaseConfiguration
            {
                AuthorizationKey = _configuration["firebase:authorizationKey"],
            });

            builder.RegisterInstance(new TelegramConfiguration
            {
                AccessToken = _configuration["telegram:accessToken"],
                Proxy = _configuration["telegram:proxy"]
            });

            builder.RegisterInstance(new CollectionGameConfiguration()
            {
                LogFile = _configuration["collectionGame:logFile"],
                LogLevel = Convert.ToInt32(_configuration["collectionGame:logLevel"]),
                TelegramManagementChatId = _configuration["collectionGame:telegramManagementChatId"]
            });

            builder.RegisterInstance(new ArtistAlleyConfiguration()
            {
                TelegramAdminGroupChatId = _configuration["artistAlley:telegram:adminGroupChatId"],
                TelegramAnnouncementChannelId = _configuration["artistAlley:telegram:announcementChannelId"],
                TwitterConsumerKey = _configuration["artistAlley:twitter:consumerKey"],
                TwitterConsumerSecret = _configuration["artistAlley:twitter:consumerSecret"],
                TwitterAccessToken = _configuration["artistAlley:twitter:accessToken"],
                TwitterAccessTokenSecret = _configuration["artistAlley:twitter:accessTokenSecret"]
            });
        }

        private void RegisterServices(ContainerBuilder builder)
        {
            builder.RegisterType<AnnouncementService>().As<IAnnouncementService>();
            builder.RegisterType<AuthenticationHandler>().As<IAuthenticationHandler>();
            builder.RegisterType<BotManager>().As<BotManager>();
            builder.RegisterType<CollectingGameService>().As<ICollectingGameService>();
            builder.RegisterType<DealerService>().As<IDealerService>();
            builder.RegisterType<EventConferenceDayService>().As<IEventConferenceDayService>();
            builder.RegisterType<EventConferenceRoomService>().As<IEventConferenceRoomService>();
            builder.RegisterType<EventConferenceTrackService>().As<IEventConferenceTrackService>();
            builder.RegisterType<EventFeedbackService>().As<IEventFeedbackService>();
            builder.RegisterType<EventService>().As<IEventService>();
            builder.RegisterType<FirebaseChannelManager>().As<IFirebaseChannelManager>();
            builder.RegisterType<FursuitBadgeService>().As<IFursuitBadgeService>();
            builder.RegisterType<ImageService>().As<IImageService>();
            builder.RegisterType<ItemActivityService>().As<IItemActivityService>();
            builder.RegisterType<KnowledgeEntryService>().As<IKnowledgeEntryService>();
            builder.RegisterType<KnowledgeGroupService>().As<IKnowledgeGroupService>();
            builder.RegisterType<LinkFragmentValidator>().As<ILinkFragmentValidator>();
            builder.RegisterType<MapService>().As<IMapService>();
            builder.RegisterType<PrivateMessageService>()
                .As<IPrivateMessageService>()
                .SingleInstance();
            builder.RegisterType<PushEventMediator>().As<IPushEventMediator>();
            builder.RegisterType<PushNotificationChannelStatisticsService>().As<IPushNotificationChannelStatisticsService>();
            builder.RegisterType<PushNotificiationChannelService>().As<IPushNotificiationChannelService>();
            builder.RegisterType<RegSysAlternativePinAuthenticationProvider>()
                .As<IRegSysAlternativePinAuthenticationProvider>();
            builder.RegisterType<StorageServiceFactory>().As<IStorageServiceFactory>();
            builder.RegisterType<TableRegistrationService>().As<ITableRegistrationService>();
            builder.RegisterType<TelegramMessageBroker>()
                .As<ITelegramMessageBroker>()
                .As<ITelegramMessageSender>()
                .SingleInstance();
            builder.RegisterType<TokenFactory>().As<ITokenFactory>();
            builder.RegisterType<UserManager>().As<IUserManager>();
            builder.RegisterType<WnsChannelManager>().As<IWnsChannelManager>();
        }
    }
}