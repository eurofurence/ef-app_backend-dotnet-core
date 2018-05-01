using Eurofurence.App.Domain.Model.Events;
using Microsoft.EntityFrameworkCore;

namespace Eurofurence.App.Domain.Model.MySql
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<EventRecord> Events { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<EventRecord>(
                entity =>
                {
                    entity.ToTable("EventEntry");
                    entity.HasKey(a => a.Id);

                    entity.HasOne(a => a.ConferenceTrack)
                        .WithMany(a => a.Events)
                        .HasForeignKey(a => a.ConferenceTrackId)
                        .HasPrincipalKey(a => a.Id);
                });

            modelBuilder.Entity<EventConferenceTrackRecord>(
                entity =>
                {
                    entity.ToTable("EventConferenceTrack");
                    entity.HasKey(a => a.Id);
                });
        }
    }

    public static class AppDbContextFactory
    {
        public static AppDbContext Create(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseMySQL(connectionString);

            //Ensure database creation
            var context = new AppDbContext(optionsBuilder.Options);
            //context.Database.EnsureCreated();

            return context;
        }
    }
}