using System;
using System.Collections.Generic;
using System.Linq;
using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Domain.Model.ArtistsAlley;
using Eurofurence.App.Domain.Model.ArtShow;
using Eurofurence.App.Domain.Model.CollectionGame;
using Eurofurence.App.Domain.Model.Communication;
using Eurofurence.App.Domain.Model.Dealers;
using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Domain.Model.Fursuits;
using Eurofurence.App.Domain.Model.Fursuits.CollectingGame;
using Eurofurence.App.Domain.Model.Images;
using Eurofurence.App.Domain.Model.Knowledge;
using Eurofurence.App.Domain.Model.LostAndFound;
using Eurofurence.App.Domain.Model.Maps;
using Eurofurence.App.Domain.Model.PushNotifications;
using Eurofurence.App.Domain.Model.Security;
using Eurofurence.App.Domain.Model.Sync;
using Eurofurence.App.Domain.Model.Telegram;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Eurofurence.App.Infrastructure.EntityFramework
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AnnouncementRecord> Announcements { get; set; }
        public virtual DbSet<TableRegistrationRecord> TableRegistrations { get; set; }
        public virtual DbSet<AgentClosingResultRecord> AgentClosingResults { get; set; }
        public virtual DbSet<ItemActivityRecord> ItemActivitys { get; set; }
        public virtual DbSet<PrivateMessageRecord> PrivateMessages { get; set; }
        public virtual DbSet<DealerRecord> Dealers { get; set; }
        public virtual DbSet<EventConferenceDayRecord> EventConferenceDays { get; set; }
        public virtual DbSet<EventConferenceTrackRecord> EventConferenceTracks { get; set; }
        public virtual DbSet<EventFeedbackRecord> EventFeedbacks { get; set; }
        public virtual DbSet<EventRecord> Events { get; set; }
        public virtual DbSet<FursuitParticipationRecord> FursuitParticipations { get; set; }
        public virtual DbSet<PlayerParticipationRecord> PlayerParticipations { get; set; }
        public virtual DbSet<CollectionEntryRecord> CollectionEntries { get; set; }
        public virtual DbSet<TokenRecord> Tokens { get; set; }
        public virtual DbSet<FursuitBadgeImageRecord> FursuitBadgeImages { get; set; }
        public virtual DbSet<FursuitBadgeRecord> FursuitBadges { get; set; }
        public virtual DbSet<ImageRecord> Images { get; set; }
        public virtual DbSet<KnowledgeEntryRecord> KnowledgeEntries { get; set; }
        public virtual DbSet<KnowledgeGroupRecord> KnowledgeGroups { get; set; }
        public virtual DbSet<LostAndFoundRecord> LostAndFounds { get; set; }
        public virtual DbSet<MapEntryRecord> MapEntries { get; set; }
        public virtual DbSet<MapRecord> Maps { get; set; }
        public virtual DbSet<PushNotificationChannelRecord> PushNotificationChannels { get; set; }
        public virtual DbSet<RegSysAccessTokenRecord> RegSysAccessTokens { get; set; }
        public virtual DbSet<RegSysAlternativePinRecord> RegSysAlternativePins { get; set; }
        public virtual DbSet<IssueRecord> IssueRecords { get; set; }
        public virtual DbSet<RegSysIdentityRecord> RegSysIdentities { get; set; }
        public virtual DbSet<EntityStorageInfoRecord> EntityStorageInfos { get; set; }
        public virtual DbSet<UserRecord> Users { get; set; }
        public virtual DbSet<RoleRecord> Roles { get; set; }
        public virtual DbSet<TopicRecord> Topics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RegSysAlternativePinRecord>().Property(x => x.PinConsumptionDatesUtc)
                .HasColumnType("json");
        }
    }
}