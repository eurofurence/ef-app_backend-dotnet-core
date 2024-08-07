using System;
using System.Threading;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions.PushNotifications
{
    public interface IPushNotificationChannelStatisticsService
    {
        Task<PushNotificationChannelStatistics> PushNotificationChannelStatisticsAsync(
            DateTime? sinceLastSeenDateTimeUtc,
            CancellationToken cancellationToken = default);
    }
}