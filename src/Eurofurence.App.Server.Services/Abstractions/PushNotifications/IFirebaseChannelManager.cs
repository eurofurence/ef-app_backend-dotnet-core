using System;
using System.Threading;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Announcements;

namespace Eurofurence.App.Server.Services.Abstractions.PushNotifications
{
    public interface IFirebaseChannelManager
    {
        Task RegisterDeviceAsync(
            string deviceToken,
            string identityId,
            string[] regSysIds,
            bool isAndroid,
            CancellationToken cancellationToken = default);

        Task PushSyncRequestAsync(CancellationToken cancellationToken = default);

        Task PushAnnouncementNotificationAsync(
            AnnouncementRecord announcement,
            CancellationToken cancellationToken = default);

        Task PushPrivateMessageNotificationToIdentityIdAsync(
            string identityId,
            string toastTitle,
            string toastMessage,
            Guid relatedId,
            CancellationToken cancellationToken = default);

        Task PushPrivateMessageNotificationToRegSysIdAsync(
            string regSysId,
            string toastTitle,
            string toastMessage,
            Guid relatedId,
            CancellationToken cancellationToken = default);
    }
}