using Eurofurence.App.Domain.Model;
using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Domain.Model.ArtistsAlley;
using Eurofurence.App.Domain.Model.ArtShow;
using Eurofurence.App.Domain.Model.CollectionGame;
using Eurofurence.App.Domain.Model.Communication;
using Eurofurence.App.Domain.Model.Dealers;
using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Domain.Model.Fragments;
using Eurofurence.App.Domain.Model.Fursuits;
using Eurofurence.App.Domain.Model.Fursuits.CollectingGame;
using Eurofurence.App.Domain.Model.Images;
using Eurofurence.App.Domain.Model.Knowledge;
using Eurofurence.App.Domain.Model.LostAndFound;
using Eurofurence.App.Domain.Model.Maps;
using Eurofurence.App.Domain.Model.Sync;
using Eurofurence.App.Domain.Model.Telegram;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using System;
using System.Linq;
using Eurofurence.App.Domain.Model.PushNotifications;

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
        // FIXME: Should be TableRegistrationRecordStateChangeRecords #EF29
        public virtual DbSet<TableRegistrationRecord.StateChangeRecord> StateChangeRecord { get; set; }
        public virtual DbSet<AgentClosingResultRecord> AgentClosingResults { get; set; }
        public virtual DbSet<ItemActivityRecord> ItemActivitys { get; set; }
        public virtual DbSet<PrivateMessageRecord> PrivateMessages { get; set; }
        public virtual DbSet<DealerRecord> Dealers { get; set; }
        public virtual DbSet<EventConferenceDayRecord> EventConferenceDays { get; set; }
        public virtual DbSet<EventConferenceRoomRecord> EventConferenceRooms { get; set; }
        public virtual DbSet<EventConferenceTrackRecord> EventConferenceTracks { get; set; }
        public virtual DbSet<EventFeedbackRecord> EventFeedbacks { get; set; }
        public virtual DbSet<EventRecord> Events { get; set; }
        public virtual DbSet<FursuitParticipationRecord> FursuitParticipations { get; set; }
        public virtual DbSet<PlayerParticipationRecord> PlayerParticipations { get; set; }
        public virtual DbSet<CollectionEntryRecord> CollectionEntries { get; set; }
        public virtual DbSet<TokenRecord> Tokens { get; set; }
        public virtual DbSet<FursuitBadgeRecord> FursuitBadges { get; set; }
        public virtual DbSet<ImageRecord> Images { get; set; }
        public virtual DbSet<KnowledgeEntryRecord> KnowledgeEntries { get; set; }
        public virtual DbSet<KnowledgeGroupRecord> KnowledgeGroups { get; set; }
        public virtual DbSet<LostAndFoundRecord> LostAndFounds { get; set; }
        public virtual DbSet<MapEntryRecord> MapEntries { get; set; }
        public virtual DbSet<MapRecord> Maps { get; set; }
        public virtual DbSet<EntityStorageInfoRecord> EntityStorageInfos { get; set; }
        public virtual DbSet<UserRecord> Users { get; set; }
        public virtual DbSet<LinkFragment> LinkFragments { get; set; }
        public virtual DbSet<DeviceIdentityRecord> DeviceIdentities { get; set; }
        public virtual DbSet<RegistrationIdentityRecord> RegistrationIdentities { get; set; }

        public virtual DbSet<ArtistAlleyUserPenaltyRecord> ArtistAlleyUserPenalties{ get; set; }

        public virtual DbSet<ArtistAlleyUserPenaltyRecord.StateChangeRecord> ArtistAlleyUserPenaltyChanges { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Global query filter for soft deleted entities
            Expression<Func<EntityBase, bool>> filterExpr = eb => eb.IsDeleted != 1;
            foreach (var mutableEntityType in modelBuilder.Model.GetEntityTypes())
            {
                if (mutableEntityType.ClrType.IsAssignableTo(typeof(EntityBase)))
                {
                    var parameter = Expression.Parameter(mutableEntityType.ClrType);
                    var body = ReplacingExpressionVisitor.Replace(filterExpr.Parameters.First(), parameter, filterExpr.Body);
                    var lambdaExpression = Expression.Lambda(body, parameter);

                    mutableEntityType.SetQueryFilter(lambdaExpression);
                }
            }

            modelBuilder.Entity<DealerRecord>().Property(x => x.Keywords)
                .HasColumnType("json");

            modelBuilder.Entity<ImageRecord>()
                .HasMany(i => i.EventPosters)
                .WithOne(e => e.PosterImage)
                .HasForeignKey(e => e.PosterImageId);

            modelBuilder.Entity<ImageRecord>()
                .HasMany(i => i.EventBanners)
                .WithOne(e => e.BannerImage)
                .HasForeignKey(e => e.BannerImageId);

            modelBuilder.Entity<ImageRecord>()
                .HasMany(i => i.DealerArtists)
                .WithOne(d => d.ArtistImage)
                .HasForeignKey(d => d.ArtistImageId);

            modelBuilder.Entity<ImageRecord>()
                .HasMany(i => i.DealerArtPreviews)
                .WithOne(d => d.ArtPreviewImage)
                .HasForeignKey(d => d.ArtPreviewImageId);

            modelBuilder.Entity<ImageRecord>()
                .HasMany(i => i.DealerArtistThumbnails)
                .WithOne(d => d.ArtistThumbnailImage)
                .HasForeignKey(d => d.ArtistThumbnailImageId);
        }
    }
}