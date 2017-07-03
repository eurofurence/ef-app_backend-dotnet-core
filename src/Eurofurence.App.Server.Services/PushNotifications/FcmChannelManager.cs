using System.Linq;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.PushNotifications;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;

namespace Eurofurence.App.Server.Services.PushNotifications
{
    public class FcmChannelManager : IFcmChannelManager
    {
        private readonly IEntityRepository<PushNotificationChannelRecord> _pushNotificationRepository;

        public FcmChannelManager(IEntityRepository<PushNotificationChannelRecord> pushNotificationRepository)
        {
            _pushNotificationRepository = pushNotificationRepository;
        }

        public async Task RegisterDeviceAsync(string deviceId, string uid, string[] topics)
        {
            var record = Enumerable.FirstOrDefault<PushNotificationChannelRecord>((await _pushNotificationRepository.FindAllAsync(a => a.DeviceId == deviceId)));

            var isNewRecord = record == null;

            if (isNewRecord)
            {
                record = new PushNotificationChannelRecord()
                {
                    Platform = PushNotificationChannelRecord.PlatformEnum.Firebase,
                    DeviceId = deviceId
                };
                record.NewId();
            }

            record.Touch();
            record.Uid = uid;
            record.Topics = topics.ToList();

            if (isNewRecord)
                await _pushNotificationRepository.InsertOneAsync(record);
            else
                await _pushNotificationRepository.ReplaceOneAsync(record);
        }
    }
}