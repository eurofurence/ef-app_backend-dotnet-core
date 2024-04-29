using Eurofurence.App.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Eurofurence.App.Server.Services.Tests
{
    public class TestDbContextFactory : IDbContextFactory<AppDbContext>
    {
        private readonly DbContextOptions<AppDbContext> _options;

        public TestDbContextFactory(string databaseName = "InMemoryTest")
        {
            _options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName)
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
        }

        public AppDbContext CreateDbContext()
        {
            var context = new AppDbContext(_options);
            context.Database.EnsureCreated();
            return context;
        }
    }
}
