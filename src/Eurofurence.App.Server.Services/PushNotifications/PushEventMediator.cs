using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Server.Services.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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
            await _firebaseChannelManager.PushAnnouncementNotificationAsync(announcement);
        }

        public async Task PushSyncRequestAsync()
        {
            await _wnsChannelManager.PushSyncRequestAsync();
            await _firebaseChannelManager.PushSyncRequestAsync();
        }
    }
}
