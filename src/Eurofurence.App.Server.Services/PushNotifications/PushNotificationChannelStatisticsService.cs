using Eurofurence.App.Server.Services.Abstractions.PushNotifications;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eurofurence.App.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Eurofurence.App.Server.Services.PushNotifications
{
    public class PushNotificationChannelStatisticsService(
        AppDbContext db
    ) : IPushNotificationChannelStatisticsService
    {
        public async Task<PushNotificationChannelStatistics> PushNotificationChannelStatisticsAsync(
            DateTime? sinceLastSeenDateTimeUtc,
            CancellationToken cancellationToken = default)
        {
            var since = sinceLastSeenDateTimeUtc ?? DateTime.UtcNow.AddMonths(-1);

            var devices = db.DeviceIdentities
                .Where(x => x.LastChangeDateTimeUtc >= since);

            var numberOfDevices = await devices
                .GroupBy(x => x.DeviceToken)
                .CountAsync(cancellationToken);

            var numberOfAuthenticatedDevices = await devices
                .Join(
                    db.RegistrationIdentities,
                    x => x.IdentityId,
                    x => x.IdentityId,
                    (x, _) => x
                )
                .GroupBy(x => x.DeviceToken)
                .CountAsync(cancellationToken);

            var numberOfUniqueUserIds = await devices
                .GroupBy(x => x.IdentityId)
                .CountAsync(cancellationToken);

            var platformStatistics = await devices
                .GroupBy(x => x.DeviceType)
                .Select(x => new PushNotificationChannelStatistics.PlatformTagInfo
                {
                    Platform = Enum.GetName(x.Key),
                    DeviceCount = x.Count(),
                })
                .ToArrayAsync(cancellationToken);

            return new PushNotificationChannelStatistics()
            {
                SinceLastSeenDateTimeUtc = since,
                NumberOfDevices = numberOfDevices,
                NumberOfAuthenticatedDevices = numberOfAuthenticatedDevices,
                NumberOfUniqueUserIds = numberOfUniqueUserIds,
                PlatformStatistics = platformStatistics
            };
        }
    }
}