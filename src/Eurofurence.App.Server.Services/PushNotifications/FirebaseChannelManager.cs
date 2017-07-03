using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Domain.Model.PushNotifications;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;
using Newtonsoft.Json;

namespace Eurofurence.App.Server.Services.PushNotifications
{
    public class FirebaseChannelManager : IFirebaseChannelManager
    {
        private readonly FirebaseConfiguration _configuration;
        private readonly IEntityRepository<PushNotificationChannelRecord> _pushNotificationRepository;

        public FirebaseChannelManager(
            FirebaseConfiguration configuration,
            IEntityRepository<PushNotificationChannelRecord> pushNotificationRepository)
        {
            _configuration = configuration;
            _pushNotificationRepository = pushNotificationRepository;
        }

        private Task<PushNotificationChannelRecord> GetRecipientChannelAsync(string recipientUid)
        {
            return _pushNotificationRepository.FindOneAsync(
                a => a.Platform == PushNotificationChannelRecord.PlatformEnum.Firebase && a.Uid == recipientUid);
        }

        public Task PushAnnouncementNotificationAsync(AnnouncementRecord announcement)
        {
            return SendPushNotificationAsync(new
            {
                Event = "Announcement",
                announcement.Title,
                Text = announcement.Content
            }, to: $"/topics/{_configuration.TargetTopic}");
        }

        public async Task PushPrivateMessageNotificationAsync(string recipientUid, string toastTitle, string toastMessage)
        {
            var recipient = await GetRecipientChannelAsync(recipientUid);
            if (recipient == null) return;

            await SendPushNotificationAsync(new
            {
                Event = "Notification",
                Title = toastTitle,
                Message = toastMessage
            }, to: recipient.DeviceId);
        }

        public Task PushSyncRequestAsync()
        {
            return SendPushNotificationAsync(new
            {
                Event = "Sync",
            }, to: $"/topics/{_configuration.TargetTopic}");
        }

        private async Task SendPushNotificationAsync(object data, string to)
        {
            var payload = new
            {
                to = to,
                data
            };

            var jsonPayload = JsonConvert.SerializeObject(payload);

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Key", $"={_configuration.AuthorizationKey}");

                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var result = await client.PostAsync("https://fcm.googleapis.com/fcm/send", content);
                result.EnsureSuccessStatusCode();
            }
        }

        public async Task RegisterDeviceAsync(string deviceId, string uid, string[] topics)
        {
            var record = (await _pushNotificationRepository.FindAllAsync(a => a.DeviceId == deviceId)).FirstOrDefault();

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