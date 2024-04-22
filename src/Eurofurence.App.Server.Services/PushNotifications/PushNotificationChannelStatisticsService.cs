using Eurofurence.App.Server.Services.Abstractions.PushNotifications;
using System;
using System.Linq;
using System.Threading.Tasks;
using Eurofurence.App.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Eurofurence.App.Server.Services.PushNotifications
{
    public class PushNotificationChannelStatisticsService : IPushNotificationChannelStatisticsService
    {
        private readonly AppDbContext _appDbContext;

        public PushNotificationChannelStatisticsService(
            AppDbContext appDbContext
        )
        {
            _appDbContext = appDbContext;
        }

        public async Task<PushNotificationChannelStatistics> PushNotificationChannelStatisticsAsync(
            DateTime? sinceLastSeenDateTimeUtc)
        {
            if (!sinceLastSeenDateTimeUtc.HasValue)
                sinceLastSeenDateTimeUtc = DateTime.UtcNow.AddMonths(-1);

            var records = await _appDbContext.PushNotificationChannels
                .Where(a => a.LastChangeDateTimeUtc >= sinceLastSeenDateTimeUtc).ToListAsync();

            var devicesWithSessions =
                records.Where(a => a.Uid.StartsWith("RegSys:", StringComparison.CurrentCultureIgnoreCase)).ToList();

            var devicesWithSessionCount = devicesWithSessions.Count();
            var uniqueUserIds = devicesWithSessions.Select(a => a.Uid).Distinct().Count();

            var groups = records
                .GroupBy(a => $"{a.Platform}-{string.Join("-", a.Topics)}")
                .Select(a => new PushNotificationChannelStatistics.PlatformTagInfo()
                {
                    Platform = a.First().Platform.ToString(),
                    Tags = a.First().Topics.ToArray(),
                    DeviceCount = a.Count()
                })
                .OrderByDescending(a => a.DeviceCount)
                .ToArray();


            return new PushNotificationChannelStatistics()
            {
                SinceLastSeenDateTimeUtc = sinceLastSeenDateTimeUtc.Value,
                NumberOfDevices = records.Count,
                NumberOfAuthenticatedDevices = devicesWithSessionCount,
                NumberOfUniqueUserIds = uniqueUserIds,
                PlatformStatistics = groups
            };
        }
    }
}