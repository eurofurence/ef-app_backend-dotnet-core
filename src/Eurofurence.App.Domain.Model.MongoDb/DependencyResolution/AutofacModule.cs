using Autofac;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Domain.Model.Images;
using Eurofurence.App.Domain.Model.Knowledge;
using Eurofurence.App.Domain.Model.MongoDb.Repositories;
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

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(r =>
                    new EntityStorageInfoRepository(
                        _mongoDatabase.GetCollection<EntityStorageInfoRecord>("EntityStorageInfoRecord")))
                .As<IEntityStorageInfoRepository>();

            builder.Register(r =>
                    new EventRepository(
                        _mongoDatabase.GetCollection<EventRecord>("EventRecord")))
                .As<IEntityRepository<EventRecord>>();

            builder.Register(r =>
                    new EventConferenceDayRepository(
                        _mongoDatabase.GetCollection<EventConferenceDayRecord>("EventConferenceDayRecord")))
                .As<IEntityRepository<EventConferenceDayRecord>>();

            builder.Register(r =>
                    new EventConferenceRoomRepository(
                        _mongoDatabase.GetCollection<EventConferenceRoomRecord>("EventConferenceRoomRecord")))
                .As<IEntityRepository<EventConferenceRoomRecord>>();

            builder.Register(r =>
                    new EventConferenceTrackRepository(
                        _mongoDatabase.GetCollection<EventConferenceTrackRecord>("EventConferenceTrackRecord")))
                .As<IEntityRepository<EventConferenceTrackRecord>>();

            builder.Register(r =>
                    new KnowledgeGroupRepository(
                        _mongoDatabase.GetCollection<KnowledgeGroupRecord>("KnowledgeGroupRecord")))
                .As<IEntityRepository<KnowledgeGroupRecord>>();

            builder.Register(r =>
                    new KnowledgeEntryRepository(
                        _mongoDatabase.GetCollection<KnowledgeEntryRecord>("KnowledgeEntryRecord")))
                .As<IEntityRepository<KnowledgeEntryRecord>>();

            builder.Register(r =>
                    new ImageRepository(
                        _mongoDatabase.GetCollection<ImageRecord>("ImageRecord")))
                .As<IEntityRepository<ImageRecord>>();

            builder.Register(r =>
                    new ImageContentRepository(
                        _mongoDatabase.GetCollection<ImageContentRecord>("ImageContentRecord")))
                .As<IEntityRepository<ImageContentRecord>>();
        }
    }
}
