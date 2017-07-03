using System;
using System.Collections.Generic;

namespace Eurofurence.App.Domain.Model.PushNotifications
{
    public class PushNotificationChannelRecord : EntityBase
    {
        public enum PlatformEnum
        {
            Wns,
            Firebase
        }

        public PushNotificationChannelRecord()
        {
            Topics = new List<string>();
        }

        public PlatformEnum Platform { get; set; }

        public string ChannelUri { get; set; }
        public string Uid { get; set; }
        public string DeviceId { get; set; }
        public IList<string> Topics { get; set; }
    }
}