using Autofac;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Announcements;
using Eurofurence.App.Server.Services.Abstractions.Communication;
using Eurofurence.App.Server.Services.Abstractions.Dealers;
using Eurofurence.App.Server.Services.Abstractions.Events;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Server.Services.Abstractions.Knowledge;
using Eurofurence.App.Server.Services.Abstractions.Maps;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Eurofurence.App.Server.Services.Abstractions.Validation;
using Eurofurence.App.Server.Services.Announcements;
using Eurofurence.App.Server.Services.Communication;
using Eurofurence.App.Server.Services.Dealers;
using Eurofurence.App.Server.Services.Events;
using Eurofurence.App.Server.Services.Images;
using Eurofurence.App.Server.Services.Knowledge;
using Eurofurence.App.Server.Services.Maps;
using Eurofurence.App.Server.Services.PushNotifications;
using Eurofurence.App.Server.Services.Security;
using Eurofurence.App.Server.Services.Storage;
using Eurofurence.App.Server.Services.Telegram;
using Eurofurence.App.Server.Services.Validation;

namespace Eurofurence.App.Server.Services.DependencyResolution
{
    public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<StorageServiceFactory>().As<IStorageServiceFactory>();
            builder.RegisterType<EventService>().As<IEventService>();
            builder.RegisterType<EventConferenceTrackService>().As<IEventConferenceTrackService>();
            builder.RegisterType<EventConferenceRoomService>().As<IEventConferenceRoomService>();
            builder.RegisterType<EventConferenceDayService>().As<IEventConferenceDayService>();
            builder.RegisterType<EventFeedbackService>().As<IEventFeedbackService>();
            builder.RegisterType<KnowledgeGroupService>().As<IKnowledgeGroupService>();
            builder.RegisterType<KnowledgeEntryService>().As<IKnowledgeEntryService>();
            builder.RegisterType<ImageService>().As<IImageService>();
            builder.RegisterType<DealerService>().As<IDealerService>();
            builder.RegisterType<AnnouncementService>().As<IAnnouncementService>();
            builder.RegisterType<MapService>().As<IMapService>();

            builder.RegisterType<TokenFactory>().As<ITokenFactory>();
            builder.RegisterType<AuthenticationHandler>().As<IAuthenticationHandler>();
            builder.RegisterType<PushNotificiationChannelService>().As<IPushNotificiationChannelService>();
            builder.RegisterType<WnsChannelManager>().As<IWnsChannelManager>();
            builder.RegisterType<PushEventMediator>().As<IPushEventMediator>();
            builder.RegisterType<FirebaseChannelManager>().As<IFirebaseChannelManager>();
            builder.RegisterType<LinkFragmentValidator>().As<ILinkFragmentValidator>();
            builder.RegisterType<PrivateMessageService>().As<IPrivateMessageService>();
            builder.RegisterType<RegSysAlternativePinAuthenticationProvider>()
                .As<IRegSysAlternativePinAuthenticationProvider>();
            builder.RegisterType<BotManager>().As<BotManager>();
        }
    }
}