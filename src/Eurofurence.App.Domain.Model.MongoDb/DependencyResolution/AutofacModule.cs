using Autofac;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Domain.Model.Communication;
using Eurofurence.App.Domain.Model.Dealers;
using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Domain.Model.Images;
using Eurofurence.App.Domain.Model.Knowledge;
using Eurofurence.App.Domain.Model.Maps;
using Eurofurence.App.Domain.Model.MongoDb.Repositories;
using Eurofurence.App.Domain.Model.PushNotifications;
using Eurofurence.App.Domain.Model.Sync;
using MongoDB.Driver;

namespace Eurofurence.App.Domain.Model.MongoDb.DependencyResolution
{
    public class AutofacModule : Module
    {
        private readonly IMongoDatabase _mongoDatabase;

        public AutofacModule(IMongoDatabase mongoDatabase)
        {
            _mongoDatabase = mongoDatabase;
        }

        private void Register<TRepository, IRepository, TRecord>(ContainerBuilder builder)
        {
            // By default, we store entities in a collection that matches the class name.
            builder.Register(r => _mongoDatabase.GetCollection<TRecord>(typeof(TRecord).Name))
                .As<IMongoCollection<TRecord>>();
            builder.RegisterType<TRepository>().As<IRepository>();
        }

        protected override void Load(ContainerBuilder builder)
        {
            Register<EntityStorageInfoRepository, IEntityStorageInfoRepository, EntityStorageInfoRecord>(builder);
            Register<EventRepository, IEntityRepository<EventRecord>, EventRecord>(builder);
            Register<EventConferenceDayRepository, IEntityRepository<EventConferenceDayRecord>, EventConferenceDayRecord
            >(builder);
            Register<EventConferenceRoomRepository, IEntityRepository<EventConferenceRoomRecord>,
                EventConferenceRoomRecord>(builder);
            Register<EventConferenceTrackRepository, IEntityRepository<EventConferenceTrackRecord>,
                EventConferenceTrackRecord>(builder);
            Register<EventFeedbackRepository, IEntityRepository<EventFeedbackRecord>, EventFeedbackRecord>(builder);
            Register<KnowledgeGroupRepository, IEntityRepository<KnowledgeGroupRecord>, KnowledgeGroupRecord>(builder);
            Register<KnowledgeEntryRepository, IEntityRepository<KnowledgeEntryRecord>, KnowledgeEntryRecord>(builder);
            Register<ImageRepository, IEntityRepository<ImageRecord>, ImageRecord>(builder);
            Register<ImageContentRepository, IEntityRepository<ImageContentRecord>, ImageContentRecord>(builder);
            Register<DealerRepository, IEntityRepository<DealerRecord>, DealerRecord>(builder);
            Register<AnnouncementRepository, IEntityRepository<AnnouncementRecord>, AnnouncementRecord>(builder);
            Register<PushNotificationChannelRepository, IEntityRepository<PushNotificationChannelRecord>,
                PushNotificationChannelRecord>(builder);
            Register<MapRepository, IEntityRepository<MapRecord>, MapRecord>(builder);
            Register<PrivateMessageRepository, IEntityRepository<PrivateMessageRecord>, PrivateMessageRecord>(builder);
        }
    }
}