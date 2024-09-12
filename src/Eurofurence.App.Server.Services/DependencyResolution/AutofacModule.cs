using System;
using Autofac;
using Autofac.Core;
using Eurofurence.App.Infrastructure.EntityFramework;
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
using Eurofurence.App.Server.Services.Abstractions.Lassie;
using Eurofurence.App.Server.Services.Abstractions.LostAndFound;
using Eurofurence.App.Server.Services.Abstractions.Maps;
using Eurofurence.App.Server.Services.Abstractions.MinIO;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;
using Eurofurence.App.Server.Services.Abstractions.QrCode;
using Eurofurence.App.Server.Services.Abstractions.Sanitization;
using Eurofurence.App.Server.Services.Abstractions.Telegram;
using Eurofurence.App.Server.Services.Abstractions.Users;
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
using Eurofurence.App.Server.Services.Lassie;
using Eurofurence.App.Server.Services.LostAndFound;
using Eurofurence.App.Server.Services.Maps;
using Eurofurence.App.Server.Services.PushNotifications;
using Eurofurence.App.Server.Services.Sanitization;
using Eurofurence.App.Server.Services.Storage;
using Eurofurence.App.Server.Services.Telegram;
using Eurofurence.App.Server.Services.Users;
using Eurofurence.App.Server.Services.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

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

            builder.RegisterInstance(ConventionSettings.FromConfiguration(_configuration));
            builder.RegisterInstance(FirebaseConfiguration.FromConfiguration(_configuration));
            builder.RegisterInstance(ApnsConfiguration.FromConfiguration(_configuration));
            builder.RegisterInstance(ExpoConfiguration.FromConfiguration(_configuration));
            builder.RegisterInstance(CollectionGameConfiguration.FromConfiguration(_configuration));
            builder.RegisterInstance(ArtistAlleyConfiguration.FromConfiguration(_configuration));
            builder.RegisterInstance(LassieConfiguration.FromConfiguration(_configuration));
            builder.RegisterInstance(MinIoConfiguration.FromConfiguration(_configuration));
            builder.RegisterInstance(DealerConfiguration.FromConfiguration(_configuration));
            builder.RegisterInstance(AnnouncementConfiguration.FromConfiguration(_configuration));
            builder.RegisterInstance(EventConfiguration.FromConfiguration(_configuration));
        }

        private void RegisterServices(ContainerBuilder builder)
        {
            var connectionString = _configuration.GetConnectionString("Eurofurence");
            var dbContextOptionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            var serverVersionString = Environment.GetEnvironmentVariable("MYSQL_VERSION");
            ServerVersion serverVersion;
            if (string.IsNullOrEmpty(serverVersionString) || !ServerVersion.TryParse(serverVersionString, out serverVersion))
            {
                serverVersion = ServerVersion.AutoDetect(connectionString);
            }

            dbContextOptionsBuilder.UseMySql(
                connectionString!,
                serverVersion,
                mySqlOptions => mySqlOptions.UseMicrosoftJson());

            builder
                .RegisterType<AppDbContext>()
                .WithParameter("options", dbContextOptionsBuilder.Options)
                .InstancePerLifetimeScope();

            builder.RegisterType<AgentClosingResultService>().As<IAgentClosingResultService>();
            builder.RegisterType<AnnouncementService>().As<IAnnouncementService>();
            builder.RegisterType<CollectingGameService>().As<ICollectingGameService>();
            builder.RegisterType<DealerService>().As<IDealerService>();
            builder.RegisterType<EventConferenceDayService>().As<IEventConferenceDayService>();
            builder.RegisterType<EventConferenceRoomService>().As<IEventConferenceRoomService>();
            builder.RegisterType<EventConferenceTrackService>().As<IEventConferenceTrackService>();
            builder.RegisterType<EventFeedbackService>().As<IEventFeedbackService>();
            builder.RegisterType<EventService>().As<IEventService>();
            //builder.RegisterType<FirebaseChannelManager>().As<IPushNotificationChannelManager>();
            builder.RegisterType<PushNotificationChannelManager>().As<IPushNotificationChannelManager>();
            builder.RegisterType<FursuitBadgeService>().As<IFursuitBadgeService>();
            builder.RegisterType<GanssHtmlSanitizer>().As<IHtmlSanitizer>();
            builder.RegisterType<ImageService>().As<IImageService>();
            builder.RegisterType<ItemActivityService>().As<IItemActivityService>();
            builder.RegisterType<KnowledgeEntryService>().As<IKnowledgeEntryService>();
            builder.RegisterType<KnowledgeGroupService>().As<IKnowledgeGroupService>();
            builder.RegisterType<LassieApiClient>().As<ILassieApiClient>();
            builder.RegisterType<LinkFragmentValidator>().As<ILinkFragmentValidator>();
            builder.RegisterType<LostAndFoundService>().As<ILostAndFoundService>();
            builder.RegisterType<LostAndFoundLassieImporter>().As<ILostAndFoundLassieImporter>();
            builder.RegisterType<MapService>().As<IMapService>();
            builder.RegisterType<PrivateMessageQueueService>().As<IPrivateMessageQueueService>().SingleInstance();
            builder.RegisterType<PrivateMessageService>().As<IPrivateMessageService>();
            builder.RegisterType<PushNotificationChannelStatisticsService>().As<IPushNotificationChannelStatisticsService>();
            builder.RegisterType<QrCodeService>().As<IQrCodeService>();
            builder.RegisterType<StorageServiceFactory>().As<IStorageServiceFactory>();
            builder.RegisterType<TableRegistrationService>().As<ITableRegistrationService>();
            builder.RegisterType<TelegramMessageBroker>()
                .As<ITelegramMessageBroker>()
                .As<ITelegramMessageSender>()
                .SingleInstance();
            builder.RegisterType<HttpUriSanitizer>().As<IHttpUriSanitizer>();
            builder.RegisterType<UserManager>().As<IUserManager>();
            builder.RegisterType<DealerApiClient>().As<IDealerApiClient>();
            builder.RegisterType<DeviceIdentityService>().As<IDeviceIdentityService>();
            builder.RegisterType<RegistrationIdentityService>().As<IRegistrationIdentityService>();
            builder.RegisterType<ArtistAlleyUserPenaltyService>().As<IArtistAlleyUserPenaltyService>();
        }
    }
}