using Eurofurence.App.Server.Services.Abstractions.PushNotifications;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eurofurence.App.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Eurofurence.App.Server.Services.PushNotifications
{
    public class PushNotificationChannelStatisticsService : IPushNotificationChannelStatisticsService
    {
        private readonly IDeviceService _deviceService;

        public PushNotificationChannelStatisticsService(IDeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        public async Task<PushNotificationChannelStatistics> PushNotificationChannelStatisticsAsync(
            DateTime? sinceLastSeenDateTimeUtc,
            CancellationToken cancellationToken = default)
        {
            var since = sinceLastSeenDateTimeUtc ?? DateTime.UtcNow.AddMonths(-1);

            var devices = await _deviceService
                .FindAll(x => x.LastChangeDateTimeUtc >= since)
                .ToListAsync(cancellationToken);

            var numberOfDevices = devices.GroupBy(x => x.DeviceToken).Count();
            var numberOfAuthenticatedDevices = devices
                .Where(x => !string.IsNullOrEmpty(x.RegSysId))
                .GroupBy(x => x.DeviceToken)
                .Count();
            var numberOfUniqueUserIds = devices.GroupBy(x => x.IdentityId).Count();

            var platformStatistics = devices
                .GroupBy(x => x.IsAndroid)
                .Select(x => new PushNotificationChannelStatistics.PlatformTagInfo
                {
                    Platform = x.Key ? "android" : "ios",
                    DeviceCount = x.Count(),
                })
                .ToArray();

            return new PushNotificationChannelStatistics()
            {
                SinceLastSeenDateTimeUtc = sinceLastSeenDateTimeUtc.Value,
                NumberOfDevices = numberOfDevices,
                NumberOfAuthenticatedDevices = numberOfAuthenticatedDevices,
                NumberOfUniqueUserIds = numberOfUniqueUserIds,
                PlatformStatistics = platformStatistics
            };
        }
    }
}