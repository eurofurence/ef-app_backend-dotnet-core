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
using Eurofurence.App.Domain.Model.PushNotifications;
using Eurofurence.App.Domain.Model.Security;
using Eurofurence.App.Domain.Model.Sync;
using Eurofurence.App.Domain.Model.Telegram;
using Microsoft.EntityFrameworkCore;

namespace Eurofurence.App.Infrastructure.EntityFramework
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<AnnouncementRecord> Announcements { get; set; }
        public DbSet<TableRegistrationRecord> TableRegistrations { get; set; }
        public DbSet<AgentClosingResultRecord> AgentClosingResults { get; set; }
        public DbSet<ItemActivityRecord> ItemActivitys { get; set; }
        public DbSet<PrivateMessageRecord> PrivateMessages { get; set; }
        public DbSet<DealerRecord> Dealers { get; set; }
        public DbSet<EventConferenceDayRecord> EventConferenceDays { get; set; }
        public DbSet<EventConferenceTrackRecord> EventConferenceTracks { get; set; }
        public DbSet<EventFeedbackRecord> EventFeedbacks { get; set; }
        public DbSet<EventRecord> Events { get; set; }
        public DbSet<FursuitParticipationRecord> FursuitParticipations { get; set; }
        public DbSet<PlayerParticipationRecord> PlayerParticipations { get; set; }
        public DbSet<TokenRecord> Tokens { get; set; }
        public DbSet<FursuitBadgeImageRecord> FursuitBadgeImages { get; set; }
        public DbSet<FursuitBadgeRecord> FursuitBadges { get; set; }
        public DbSet<ImageContentRecord> ImageContents { get; set; }
        public DbSet<ImageRecord> Images { get; set; }
        public DbSet<KnowledgeEntryRecord> KnowledgeEntries { get; set; }
        public DbSet<KnowledgeGroupRecord> KnowledgeGroups { get; set; }
        public DbSet<LostAndFoundRecord> LostAndFounds { get; set; }
        public DbSet<MapEntryRecord> MapEntries { get; set; }
        public DbSet<MapRecord> Maps { get; set; }
        public DbSet<PushNotificationChannelRecord> PushNotificationChannels { get; set; }
        public DbSet<RegSysAccessTokenRecord> RegSysAccessTokens { get; set; }
        public DbSet<RegSysAlternativePinRecord> RegSysAlternativePins { get; set; }
        public DbSet<RegSysIdentityRecord> RegSysIdentities { get; set; }
        public DbSet<EntityStorageInfoRecord> EntityStorageInfos { get; set; }
        public DbSet<UserRecord> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}