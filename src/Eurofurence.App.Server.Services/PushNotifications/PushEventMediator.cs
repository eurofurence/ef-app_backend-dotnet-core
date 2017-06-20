using Eurofurence.App.Domain.Model.Announcements;
using System;
using System.Threading.Tasks;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;

namespace Eurofurence.App.Server.Services.PushNotifications
{
    public class PushEventMediator : IPushEventMediator
    {
        readonly IFirebaseChannelManager _firebaseChannelManager;
        readonly IWnsChannelManager _wnsChannelManager;

        public PushEventMediator(IWnsChannelManager wnsChannelManager, IFirebaseChannelManager firebaseChannelManager)
        {
            _wnsChannelManager = wnsChannelManager;
            _firebaseChannelManager = firebaseChannelManager;
        }

        public async Task PushAnnouncementNotificationAsync(AnnouncementRecord announcement)
        {
            await _wnsChannelManager.PushAnnouncementNotificationAsync(announcement);

            if (announcement.ValidFromDateTimeUtc <= DateTime.UtcNow)
            {
                await _firebaseChannelManager.PushAnnouncementNotificationAsync(announcement);
            }
        }

        public async Task PushSyncRequestAsync()
        {
            await _wnsChannelManager.PushSyncRequestAsync();
            await _firebaseChannelManager.PushSyncRequestAsync();
        }
    }
}
