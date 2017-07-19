using System;
using Autofac;
using Eurofurence.App.Common.Abstractions;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Domain.Model.ArtShow;
using Eurofurence.App.Domain.Model.Communication;
using Eurofurence.App.Domain.Model.Dealers;
using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Domain.Model.Fursuits;
using Eurofurence.App.Domain.Model.Fursuits.CollectingGame;
using Eurofurence.App.Domain.Model.Images;
using Eurofurence.App.Domain.Model.Knowledge;
using Eurofurence.App.Domain.Model.Maps;
using Eurofurence.App.Domain.Model.MongoDb.ArtShow;
using Eurofurence.App.Domain.Model.MongoDb.Fursuits;
using Eurofurence.App.Domain.Model.MongoDb.Fursuits.CollectingGame;
using Eurofurence.App.Domain.Model.MongoDb.Repositories;
using Eurofurence.App.Domain.Model.MongoDb.Security;
using Eurofurence.App.Domain.Model.MongoDb.Telegram;
using Eurofurence.App.Domain.Model.PushNotifications;
using Eurofurence.App.Domain.Model.Security;
using Eurofurence.App.Domain.Model.Sync;
using Eurofurence.App.Domain.Model.Telegram;
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

        private void Register<TRepository, IRepository, TRecord>(
            ContainerBuilder builder, 
            Action<IMongoCollection<TRecord>> setup = null
            ) where TRecord: IEntityBase
        {
            var name = typeof(TRecord).FullName.Replace("Eurofurence.App.Domain.Model.", "");
            var collection = _mongoDatabase.GetCollection<TRecord>(name);

            builder.Register(r => collection).As<IMongoCollection<TRecord>>();
            builder.RegisterType<TRepository>().As<IRepository>();

            collection.Indexes.CreateOne(Builders<TRecord>.IndexKeys.Ascending(a => a.IsDeleted));
            collection.Indexes.CreateOne(Builders<TRecord>.IndexKeys.Descending(a => a.LastChangeDateTimeUtc));

            setup?.Invoke(collection);
        }

        protected override void Load(ContainerBuilder builder)
        {
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

            Register<RegSysAlternativePinRepository, IEntityRepository<RegSysAlternativePinRecord>, RegSysAlternativePinRecord>(builder);
            Register<RegSysIdentityRepository, IEntityRepository<RegSysIdentityRecord>, RegSysIdentityRecord>(builder,
                collection => collection.Indexes.CreateOne(
                    Builders<RegSysIdentityRecord>.IndexKeys.Ascending(a => a.Uid)));

            Register<UserRepository, IEntityRepository<UserRecord>, UserRecord>(builder);

            Register<FursuitBadgeRepository, IEntityRepository<FursuitBadgeRecord>, FursuitBadgeRecord>(builder,
                collection => collection.Indexes.CreateOne(
                    Builders<FursuitBadgeRecord>.IndexKeys.Ascending(a => a.OwnerUid)));

            Register<FursuitBadgeImageRepository, IEntityRepository<FursuitBadgeImageRecord>, FursuitBadgeImageRecord>(builder);

            Register<PlayerParticipantRepository, IEntityRepository<PlayerParticipationRecord>, PlayerParticipationRecord>(builder,
                collection => collection.Indexes.CreateOne(
                    Builders<PlayerParticipationRecord>.IndexKeys.Ascending(a => a.PlayerUid)));

            Register<FursuitParticipantRepository, IEntityRepository<FursuitParticipationRecord>, FursuitParticipationRecord>(builder,
                collection =>
                {
                    collection.Indexes.CreateOne(
                        Builders<FursuitParticipationRecord>.IndexKeys.Ascending(a => a.OwnerUid));
                    collection.Indexes.CreateOne(
                        Builders<FursuitParticipationRecord>.IndexKeys.Ascending(a => a.TokenValue));
                });

            Register<TokenRepository, IEntityRepository<TokenRecord>, TokenRecord>(builder,
                collection => collection.Indexes.CreateOne(
                    Builders<TokenRecord>.IndexKeys.Ascending(a => a.Value)));

            Register<ItemActivityRepository, IEntityRepository<ItemActivityRecord>, ItemActivityRecord>(builder,
                collection => collection.Indexes.CreateOne(
                    Builders<ItemActivityRecord>.IndexKeys.Ascending(a => a.ImportHash)));
        }
    }
}