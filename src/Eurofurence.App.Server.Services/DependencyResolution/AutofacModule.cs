using Autofac;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Events;
using Eurofurence.App.Server.Services.Storage;

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
            builder.RegisterType<KnowledgeGroupService>().As<IKnowledgeGroupService>();
            builder.RegisterType<KnowledgeEntryService>().As<IKnowledgeEntryService>();
        }
    }
}
