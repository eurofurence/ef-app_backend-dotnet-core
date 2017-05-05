using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.PushNotifications;
using Eurofurence.App.Server.Services.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.PushNotifications
{
    public class WnsChannelManager : IWnsChannelManager
    {
        readonly IEntityRepository<PushNotificationChannelRecord> _pushNotificationRepository;

        public WnsChannelManager(
            IEntityRepository<PushNotificationChannelRecord> pushNotificationRepository)
        {
            _pushNotificationRepository = pushNotificationRepository;
        }

        public async Task RegisterChannelAsync(Guid deviceId, string channelUri, string uid, string[] topics)
        {
            var record = (await _pushNotificationRepository.FindAllAsync(a => a.DeviceId == deviceId))
                .FirstOrDefault();

            bool isNewRecord = record == null;

            if (isNewRecord)
            {
                record = new PushNotificationChannelRecord();
                record.DeviceId = deviceId;
                record.NewId();
            }

            record.Touch();
            record.ChannelUri = channelUri;
            record.Uid = uid;
            record.Topics = topics.ToList();
            
            if (isNewRecord)
            {
                await _pushNotificationRepository.InsertOneAsync(record);
            }
            else
            {
                await _pushNotificationRepository.ReplaceOneAsync(record);
            }
        }
    }
}
