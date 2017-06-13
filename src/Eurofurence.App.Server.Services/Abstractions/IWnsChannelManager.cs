using Eurofurence.App.Domain.Model.Announcements;
using System;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public interface IWnsChannelManager
    {
        Task RegisterChannelAsync(Guid deviceId, string channelUri, string uid, string[] topics);

        Task SendToastAsync(string topic, string message);

        Task PushSyncUpdateRequestAsync(string topic);

        Task PushAnnouncementAsync(string topic, AnnouncementRecord announcement);

        Task PushPrivateMessageNotificationAsync(string recipientUid);
    }
}