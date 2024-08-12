using System;

namespace Eurofurence.App.Server.Services.Abstractions.PushNotifications
{
    public class PushNotificationChannelStatistics
    {
        public class PlatformTagInfo
        {
            public string Platform { get; set; }
            public int DeviceCount { get; set; }
        }

        public DateTime SinceLastSeenDateTimeUtc { get; set; }
        public int NumberOfDevices { get; set; }
        public int NumberOfAuthenticatedDevices { get; set; }
        public int NumberOfUniqueUserIds { get; set; }

        public PlatformTagInfo[] PlatformStatistics { get; set; }
    }
}