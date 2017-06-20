using System;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Announcements;

namespace Eurofurence.App.Server.Services.Abstractions.PushNotifications
{
    public interface IWnsChannelManager
    {
        Task RegisterChannelAsync(Guid deviceId, string channelUri, string uid, string[] topics);

        Task SendToastAsync(string topic, string message);

        Task PushSyncRequestAsync();

        Task PushAnnouncementNotificationAsync(AnnouncementRecord announcement);

        Task PushPrivateMessageNotificationAsync(string recipientUid);
    }
}