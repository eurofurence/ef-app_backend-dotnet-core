using Autofac;
using Eurofurence.App.Common.Abstractions;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Domain.Model.ArtistsAlley;
using Eurofurence.App.Domain.Model.ArtShow;
using Eurofurence.App.Domain.Model.Communication;
using Eurofurence.App.Domain.Model.Dealers;
using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Domain.Model.Fursuits;
using Eurofurence.App.Domain.Model.Fursuits.CollectingGame;
using Eurofurence.App.Domain.Model.Images;
using Eurofurence.App.Domain.Model.Knowledge;
using Eurofurence.App.Domain.Model.LostAndFound;
using Eurofurence.App.Domain.Model.Maps;
using Eurofurence.App.Domain.Model.Meetups;
using Eurofurence.App.Domain.Model.MongoDb.ArtShow;
using Eurofurence.App.Domain.Model.MongoDb.Fursuits;
using Eurofurence.App.Domain.Model.MongoDb.Fursuits.CollectingGame;
using Eurofurence.App.Domain.Model.MongoDb.LostAndFound;
using Eurofurence.App.Domain.Model.MongoDb.Repositories;
using Eurofurence.App.Domain.Model.MongoDb.Security;
using Eurofurence.App.Domain.Model.MongoDb.Telegram;
using Eurofurence.App.Domain.Model.PushNotifications;
using Eurofurence.App.Domain.Model.Security;
using Eurofurence.App.Domain.Model.Sync;
using Eurofurence.App.Domain.Model.Telegram;
using MongoDB.Driver;
using System;

namespace Eurofurence.App.Domain.Model.MongoDb.DependencyResolution
{
    public class AutofacModule : Module
    {
        private MongoDatabaseBroker _mongoDatabaseInitialization;

        public AutofacModule()
        {
            _mongoDatabaseInitialization = new MongoDatabaseBroker();
        }

        private void Register<TRepository, IRepository, TRecord>(
            ContainerBuilder builder,
            Action<IMongoCollection<TRecord>> setup = null
            ) where TRecord : IEntityBase
        {
            var name = typeof(TRecord).FullName.Replace("Eurofurence.App.Domain.Model.", "");

            builder.Register(_ => _mongoDatabaseInitialization.GetCollection<TRecord>(name))
                .As<IMongoCollection<TRecord>>();

            builder.RegisterType<TRepository>().As<IRepository>();

            _mongoDatabaseInitialization.InitializationTasks.Add(mongoDatabase =>
            {
                var collection = mongoDatabase.GetCollection<TRecord>(name);

                var createBasicIndexes = new Action(() =>
                {
                    collection.Indexes.CreateOne(new CreateIndexModel<TRecord>(Builders<TRecord>.IndexKeys.Ascending(a => a.IsDeleted)));
                    collection.Indexes.CreateOne(new CreateIndexModel<TRecord>(Builders<TRecord>.IndexKeys.Descending(a => a.LastChangeDateTimeUtc)));
                });

                try
                {
                    createBasicIndexes();
                    setup?.Invoke(collection);
                }
                catch (MongoWriteConcernException)
                {
                    collection.Indexes.DropAll();
                    createBasicIndexes();
                    setup?.Invoke(collection);
                }
            });
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance<IMongoDatabaseBroker>(_mongoDatabaseInitialization);

            Register<EntityStorageInfoRepository, IEntityStorageInfoRepository, EntityStorageInfoRecord>(builder);
            Register<EventRepository, IEntityRepository<EventRecord>, EventRecord>(builder);
            Register<EventConferenceDayRepository, IEntityRepository<EventConferenceDayRecord>, EventConferenceDayRecord>(builder);
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
            Register<MeetupRepository, IEntityRepository<Meetup>, Meetup>(builder);

            Register<RegSysAlternativePinRepository, IEntityRepository<RegSysAlternativePinRecord>, RegSysAlternativePinRecord>(builder);
            Register<RegSysIdentityRepository, IEntityRepository<RegSysIdentityRecord>, RegSysIdentityRecord>(builder,
                collection => collection.Indexes.CreateOne(
                    new CreateIndexModel<RegSysIdentityRecord>(
                        Builders<RegSysIdentityRecord>.IndexKeys.Ascending(a => a.Uid))));
            Register<RegSysAccessTokenRepository, IEntityRepository<RegSysAccessTokenRecord>, RegSysAccessTokenRecord>(builder,
                collection => collection.Indexes.CreateOne(
                    new CreateIndexModel<RegSysAccessTokenRecord>(
                        Builders<RegSysAccessTokenRecord>.IndexKeys.Ascending(a => a.Token))));

            Register<UserRepository, IEntityRepository<UserRecord>, UserRecord>(builder);

            Register<FursuitBadgeRepository, IEntityRepository<FursuitBadgeRecord>, FursuitBadgeRecord>(builder,
                collection =>
                {
                    collection.Indexes.CreateOne(
                        new CreateIndexModel<FursuitBadgeRecord>(
                            Builders<FursuitBadgeRecord>.IndexKeys.Ascending(a => a.ExternalReference),
                            new CreateIndexOptions() { Unique = true }));
                    collection.Indexes.CreateOne(
                        new CreateIndexModel<FursuitBadgeRecord>(
                            Builders<FursuitBadgeRecord>.IndexKeys.Ascending(a => a.OwnerUid)));
                });

            Register<FursuitBadgeImageRepository, IEntityRepository<FursuitBadgeImageRecord>, FursuitBadgeImageRecord>(builder);

            Register<PlayerParticipantRepository, IEntityRepository<PlayerParticipationRecord>, PlayerParticipationRecord>(builder,
                collection =>
                {
                    collection.Indexes.CreateOne(
                        new CreateIndexModel<PlayerParticipationRecord>(
                            Builders<PlayerParticipationRecord>.IndexKeys.Ascending(a => a.PlayerUid),
                            new CreateIndexOptions() { Unique = true }));
                    collection.Indexes.CreateOne(
                            new CreateIndexModel<PlayerParticipationRecord>(
                            Builders<PlayerParticipationRecord>.IndexKeys.Descending(a => a.CollectionCount)));
                    collection.Indexes.CreateOne(
                            new CreateIndexModel<PlayerParticipationRecord>(
                            Builders<PlayerParticipationRecord>.IndexKeys.Ascending(a => a.LastCollectionDateTimeUtc)));
                    collection.Indexes.CreateOne(
                            new CreateIndexModel<PlayerParticipationRecord>(
                            Builders<PlayerParticipationRecord>.IndexKeys.Ascending(a => a.IsBanned)));
                });

            Register<FursuitParticipantRepository, IEntityRepository<FursuitParticipationRecord>, FursuitParticipationRecord>(builder,
                collection =>
                {
                    collection.Indexes.CreateOne(
                        new CreateIndexModel<FursuitParticipationRecord>(
                            Builders<FursuitParticipationRecord>.IndexKeys.Ascending(a => a.OwnerUid)));
                    collection.Indexes.CreateOne(
                        new CreateIndexModel<FursuitParticipationRecord>(
                            Builders<FursuitParticipationRecord>.IndexKeys.Ascending(a => a.TokenValue),
                            new CreateIndexOptions() { Unique = true }));
                    collection.Indexes.CreateOne(
                        new CreateIndexModel<FursuitParticipationRecord>(
                            Builders<FursuitParticipationRecord>.IndexKeys.Descending(a => a.CollectionCount)));
                    collection.Indexes.CreateOne(
                        new CreateIndexModel<FursuitParticipationRecord>(
                            Builders<FursuitParticipationRecord>.IndexKeys.Ascending(a => a.LastCollectionDateTimeUtc)));
                });

            Register<TokenRepository, IEntityRepository<TokenRecord>, TokenRecord>(builder,
                collection => collection.Indexes.CreateOne(
                    new CreateIndexModel<TokenRecord>(
                        Builders<TokenRecord>.IndexKeys.Ascending(a => a.Value),
                        new CreateIndexOptions() { Unique = true })));

            Register<ItemActivityRepository, IEntityRepository<ItemActivityRecord>, ItemActivityRecord>(builder,
                collection => collection.Indexes.CreateOne(
                    new CreateIndexModel<ItemActivityRecord>(
                        Builders<ItemActivityRecord>.IndexKeys.Ascending(a => a.ImportHash))));

            Register<AgentClosingResultRepository, IEntityRepository<AgentClosingResultRecord>, AgentClosingResultRecord>(builder,
                collection => collection.Indexes.CreateOne(
                    new CreateIndexModel<AgentClosingResultRecord>(
                        Builders<AgentClosingResultRecord>.IndexKeys.Ascending(a => a.ImportHash))));

            Register<TableRegistrationRepository, IEntityRepository<TableRegistrationRecord>, TableRegistrationRecord>(builder);
            Register<LostAndFoundRepository, IEntityRepository<LostAndFoundRecord>, LostAndFoundRecord>(builder);
        }
    }
}