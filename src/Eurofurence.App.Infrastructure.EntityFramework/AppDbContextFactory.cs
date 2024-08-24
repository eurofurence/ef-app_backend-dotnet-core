using System;
using System.IO;
using Eurofurence.App.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Get environment
        string environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        // Build config
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .Build();

        // Get connection string
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        var connectionString = config.GetConnectionString("Eurofurence");

        var serverVersionString = Environment.GetEnvironmentVariable("MYSQL_VERSION");
        ServerVersion serverVersion;
        if (string.IsNullOrEmpty(serverVersionString) || !ServerVersion.TryParse(serverVersionString, out serverVersion))
        {
            serverVersion = ServerVersion.AutoDetect(connectionString);
        }

        optionsBuilder.UseMySql(
                    connectionString,
                    serverVersion,
                    mySqlOptions => mySqlOptions.UseMicrosoftJson());
        return new AppDbContext(optionsBuilder.Options);
    }
}