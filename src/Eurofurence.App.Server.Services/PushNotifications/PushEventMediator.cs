using System;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;

namespace Eurofurence.App.Server.Services.PushNotifications
{
    public class PushEventMediator : IPushEventMediator
    {
        private readonly IFirebaseChannelManager _firebaseChannelManager;
        private readonly IWnsChannelManager _wnsChannelManager;

        public PushEventMediator(IWnsChannelManager wnsChannelManager, IFirebaseChannelManager firebaseChannelManager)
        {
            _wnsChannelManager = wnsChannelManager;
            _firebaseChannelManager = firebaseChannelManager;
        }

        public async Task PushAnnouncementNotificationAsync(AnnouncementRecord announcement)
        {
            await _wnsChannelManager.PushAnnouncementNotificationAsync(announcement);

            if (announcement.ValidFromDateTimeUtc <= DateTime.UtcNow)
                await _firebaseChannelManager.PushAnnouncementNotificationAsync(announcement);
        }

        public async Task PushPrivateMessageNotificationAsync(string recipientUid, string toastTitle, string toastMessage, Guid relatedId)
        {
            await _wnsChannelManager.PushPrivateMessageNotificationAsync(recipientUid, toastTitle, toastMessage);
            await _firebaseChannelManager.PushPrivateMessageNotificationAsync(recipientUid, toastTitle, toastMessage, relatedId);
        }

        public async Task PushSyncRequestAsync()
        {
            await _wnsChannelManager.PushSyncRequestAsync();
            await _firebaseChannelManager.PushSyncRequestAsync();
        }
    }
}