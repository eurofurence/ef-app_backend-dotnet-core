using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Eurofurence.App.Common.ExtensionMethods;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Domain.Model.PushNotifications;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;
using Newtonsoft.Json;

namespace Eurofurence.App.Server.Services.PushNotifications
{
    public class FirebaseChannelManager : IFirebaseChannelManager
    {
        private readonly FirebaseConfiguration _configuration;
        private readonly IEntityRepository<PushNotificationChannelRecord> _pushNotificationRepository;
        private readonly ConventionSettings _conventionSettings;

        public FirebaseChannelManager(
            FirebaseConfiguration configuration,
            IEntityRepository<PushNotificationChannelRecord> pushNotificationRepository,
            ConventionSettings conventionSettings
            )

        {
            _configuration = configuration;
            _pushNotificationRepository = pushNotificationRepository;
            _conventionSettings = conventionSettings;
        }

        private Task<IEnumerable<PushNotificationChannelRecord>> GetRecipientChannelAsync(string recipientUid)
        {
            return _pushNotificationRepository.FindAllAsync(
                a => a.Platform == PushNotificationChannelRecord.PlatformEnum.Firebase && a.Uid == recipientUid);
        }

        public Task PushAnnouncementNotificationAsync(AnnouncementRecord announcement)
        {
            return Task.WhenAll(
                SendPushNotificationAsync(new
                {
                    data = new
                    {
                        Event = "Announcement",
                        Title = announcement.Title.RemoveMarkdown(),
                        Text = announcement.Content.RemoveMarkdown(),
                        RelatedId = announcement.Id,
                        CID = _conventionSettings.ConventionIdentifier
                    },
                    to = $"/topics/{_conventionSettings.ConventionIdentifier}-android"
                }), 
                SendPushNotificationAsync(new
                {
                    data = new
                    {
                        @event = "announcement",
                        announcement_id = announcement.Id
                    },
                    notification = new {
                        title = announcement.Title.RemoveMarkdown(),
                        body = announcement.Content.RemoveMarkdown(),
                        sound = "notification_default.caf"
                    },
                    content_available = true,
                    priority = "high",
                    to = $"/topics/{_conventionSettings.ConventionIdentifier}-ios"
                })
            );
        }

        public async Task PushPrivateMessageNotificationAsync(string recipientUid, string toastTitle, string toastMessage, Guid relatedId)
        {
            var recipients = await GetRecipientChannelAsync(recipientUid);

            foreach (var recipient in recipients)
            {
                if (recipient.Topics.Contains("ios", StringComparer.CurrentCultureIgnoreCase))
                {
                    await SendPushNotificationAsync(new
                    {
                        data = new
                        {
                            @event = "notification",
                            message_id = relatedId
                        },
                        notification = new
                        {
                            title = toastTitle,
                            body = toastMessage,
                            sound = "notification_default.caf"
                        },
                        to = recipient.DeviceId
                    });
                }
                else
                {
                    await SendPushNotificationAsync(new
                    {
                        data = new
                        {
                            Event = "Notification",
                            Title = toastTitle,
                            Message = toastMessage,
                            RelatedId = relatedId,
                            CID = _conventionSettings.ConventionIdentifier
                        },
                        to = recipient.DeviceId
                    });
                }
            }
        }

        public Task PushSyncRequestAsync()
        {
            return Task.WhenAll(
                SendPushNotificationAsync(new
                {
                    data = new
                    {
                        Event = "Sync",
                        CID = _conventionSettings.ConventionIdentifier
                    },
                    to = $"/topics/{_conventionSettings.ConventionIdentifier}-android"
                }),
                SendPushNotificationAsync(new
                {
                    data = new
                    {
                        @event = "sync",
                    },
                    content_available = true,
                    priority = "high",
                    to = $"/topics/{_conventionSettings.ConventionIdentifier}-ios"
                })
            );
        }

        private async Task SendPushNotificationAsync(object payload)
        {

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