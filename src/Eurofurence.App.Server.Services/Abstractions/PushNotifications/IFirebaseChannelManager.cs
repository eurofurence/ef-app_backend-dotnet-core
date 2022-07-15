using System;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Announcements;

namespace Eurofurence.App.Server.Services.Abstractions.PushNotifications
{
    public interface IFirebaseChannelManager
    {
        Task RegisterDeviceAsync(string deviceId, string uid, string[] topics);

        Task PushSyncRequestAsync();

        Task PushAnnouncementNotificationAsync(AnnouncementRecord announcement);

        Task PushPrivateMessageNotificationAsync(string recipientUid, string toastTitle, string toastMessage, Guid relatedId);

        Task SubscribeToTopicAsync(string deviceId, string topic);
        Task UnsubscribeFromTopicAsync(string deviceId, string topic);
    }
}