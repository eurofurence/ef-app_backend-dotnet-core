using System;
using System.Threading;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Domain.Model.PushNotifications;

namespace Eurofurence.App.Server.Services.Abstractions.PushNotifications
{
    public interface IPushNotificationChannelManager
    {
        Task RegisterDeviceAsync(
            string deviceToken,
            string identityId,
            string[] regSysIds,
            DeviceType type,
            CancellationToken cancellationToken = default);

        Task PushSyncRequestAsync(CancellationToken cancellationToken = default);

        Task PushAnnouncementNotificationAsync(
            AnnouncementRecord announcement,
            CancellationToken cancellationToken = default);

        Task PushPrivateMessageNotificationToIdentityIdAsync(
            string identityId,
            string title,
            string message,
            Guid relatedId,
            CancellationToken cancellationToken = default);

        Task PushPrivateMessageNotificationToRegSysIdAsync(
            string regSysId,
            string title,
            string message,
            Guid relatedId,
            CancellationToken cancellationToken = default);
    }
}