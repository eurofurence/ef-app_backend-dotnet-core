using System;
using System.Threading.Tasks;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;

namespace Eurofurence.App.Server.Services.Abstractions.PushNotifications
{
    public interface IPushNotificationChannelStatisticsService
    {
        Task<PushNotificationChannelStatistics> PushNotificationChannelStatisticsAsync(DateTime? sinceLastSeenDateTimeUtc);
    }
}