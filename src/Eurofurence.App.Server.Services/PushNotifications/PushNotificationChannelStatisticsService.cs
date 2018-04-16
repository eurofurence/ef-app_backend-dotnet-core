using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.PushNotifications;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.PushNotifications
{
    public class PushNotificationChannelStatisticsService : IPushNotificationChannelStatisticsService
    {
        private readonly IEntityRepository<PushNotificationChannelRecord> _pushNotificationChannelRepository;

        public PushNotificationChannelStatisticsService(
            IEntityRepository<PushNotificationChannelRecord> pushNotificationChannelRepository
        )
        {
            _pushNotificationChannelRepository = pushNotificationChannelRepository;
        }

        public async Task<PushNotificationChannelStatistics> PushNotificationChannelStatisticsAsync(DateTime? sinceLastSeenDateTimeUtc)
        {
            if (!sinceLastSeenDateTimeUtc.HasValue)
                sinceLastSeenDateTimeUtc = DateTime.UtcNow.AddMonths(-1);

            var records = (await _pushNotificationChannelRepository.FindAllAsync())
                .Where(a => a.LastChangeDateTimeUtc >= sinceLastSeenDateTimeUtc)
                .ToList();

            var devicesWithSessions =
                records.Where(a => a.Uid.StartsWith("RegSys:", StringComparison.CurrentCultureIgnoreCase));

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
                NumberOfDevices = devicesWithSessionCount,
                NumberOfAuthenticatedDevices = devicesWithSessionCount,
                NumberOfUniqueUserIds = uniqueUserIds,
                PlatformStatistics = groups
            };
        }
    }
}
