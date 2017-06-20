using System;
using System.Collections.Generic;

namespace Eurofurence.App.Domain.Model.PushNotifications
{
    public class PushNotificationChannelRecord : EntityBase
    {
        public PushNotificationChannelRecord()
        {
            Topics = new List<string>();
        }

        public string ChannelUri { get; set; }
        public string Uid { get; set; }
        public Guid DeviceId { get; set; }
        public IList<string> Topics { get; set; }
    }
}