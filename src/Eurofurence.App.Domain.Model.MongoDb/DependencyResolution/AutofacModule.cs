using Autofac;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Events;
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

        protected override void Load(ContainerBuilder moduleBuilder)
        {
            moduleBuilder.Register(r =>
                    new EntityStorageInfoRepository(
                        _mongoDatabase.GetCollection<EntityStorageInfoRecord>("EntityStorageInfoRecord")))
                .As<IEntityStorageInfoRepository>();

            moduleBuilder.Register(r =>
                    new EventRepository(
                        _mongoDatabase.GetCollection<EventRecord>("EventRecord")))
                .As<IEntityRepository<EventRecord>>();

            moduleBuilder.Register(r =>
                    new EventConferenceDayRepository(
                        _mongoDatabase.GetCollection<EventConferenceDayRecord>("EventConferenceDayRecord")))
                .As<IEntityRepository<EventConferenceDayRecord>>();

            moduleBuilder.Register(r =>
                    new EventConferenceRoomRepository(
                        _mongoDatabase.GetCollection<EventConferenceRoomRecord>("EventConferenceRoomRecord")))
                .As<IEntityRepository<EventConferenceRoomRecord>>();

            moduleBuilder.Register(r =>
                    new EventConferenceTrackRepository(
                        _mongoDatabase.GetCollection<EventConferenceTrackRecord>("EventConferenceTrackRecord")))
                .As<IEntityRepository<EventConferenceTrackRecord>>();
        }
    }
}
