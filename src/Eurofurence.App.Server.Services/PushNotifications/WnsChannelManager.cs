using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Domain.Model.PushNotifications;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Eurofurence.App.Server.Services.PushNotifications
{
    public class WnsChannelManager : IWnsChannelManager
    {
        private readonly AppDbContext _appDbContext;
        private readonly WnsConfiguration _configuration;

        public WnsChannelManager(
            AppDbContext appDbContext,
            WnsConfiguration configuration
        )
        {
            _appDbContext = appDbContext;
            _configuration = configuration;
        }

        public async Task PushSyncRequestAsync()
        {
            var recipients = GetAllRecipientsAsyncByTopic(_configuration.TargetTopic);
            await SendRawAsync(recipients, "update");
        }

        public async Task PushAnnouncementNotificationAsync(AnnouncementRecord announcement)
        {
            var message = new
            {
                Event = "NewAnnouncement",
                Content = announcement
            };

            var recipients = GetAllRecipientsAsyncByTopic(_configuration.TargetTopic);
            await SendRawAsync(recipients, JsonSerializer.Serialize(message));
        }

        public async Task SendToastAsync(string topic, string message)
        {
            var recipients = GetAllRecipientsAsyncByTopic(topic);
            var accessToken = await GetWnsAccessTokenAsync();

            foreach (var recipient in recipients)
            {
#pragma warning disable CS4014
                Task.Run(async () =>
                {
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
                        client.DefaultRequestHeaders.Add("X-WNS-TYPE", "wns/toast");

                        var xml =
                            $"<?xml version=\"1.0\" encoding=\"utf-8\"?><toast><visual><binding template=\"ToastText01\"><text id=\"1\">{message}</text></binding></visual></toast>";
                        var payload = new StringContent(xml, Encoding.UTF8, "text/xml");

                        var result = await client.PostAsync(recipient.ChannelUri, payload);

                        if (!result.IsSuccessStatusCode)
                        {
                            var pushNotificationToDelete =
                                await _appDbContext.PushNotificationChannels.FirstOrDefaultAsync(entity =>
                                    entity.Id == recipient.Id);
                            if (pushNotificationToDelete != null)
                            {
                                _appDbContext.Remove(pushNotificationToDelete);
                                _appDbContext.SaveChangesAsync();
                            }
                        }
                    }
                });
#pragma warning restore CS4014
            }
        }

        public async Task RegisterChannelAsync(string deviceId, string channelUri, string uid, string[] topics)
        {
            var record = await _appDbContext.PushNotificationChannels.FirstOrDefaultAsync(a => a.DeviceId == deviceId);

            var isNewRecord = record == null;

            if (isNewRecord)
            {
                record = new PushNotificationChannelRecord()
                {
                    Platform = PushNotificationChannelRecord.PlatformEnum.Wns,
                    DeviceId = deviceId
                };
                record.NewId();
            }

            record.Touch();
            record.ChannelUri = channelUri;
            record.Uid = uid;

            foreach (var topic in topics)
            {
                var topicRecord = await _appDbContext.Topics.FirstOrDefaultAsync(t => t.Name == topic);

                if (topicRecord == null)
                {
                    topicRecord = new TopicRecord
                    {
                        Name = topic
                    };
                    _appDbContext.Topics.Add(topicRecord);
                }
                
                record.Topics.Add(topicRecord);
            }

            if (isNewRecord)
                _appDbContext.PushNotificationChannels.Add(record);
            else
                _appDbContext.PushNotificationChannels.Update(record);

            await _appDbContext.SaveChangesAsync();
        }

        public async Task PushPrivateMessageNotificationAsync(string recipientUid, string toastTitle, string toastMessage)
        {
            var recipients = GetAllRecipientsAsyncByUid(recipientUid);
            await SendRawAsync(recipients, new
            {
                Event = "PrivateMessage_Received",
                Content = new
                {
                    ToastTitle = toastTitle,
                    ToastMessage = toastMessage
                }
            });
        }

        private IQueryable<PushNotificationChannelRecord> GetAllRecipients()
        {
            return _appDbContext.PushNotificationChannels
                .AsNoTracking()
                .Where(a => a.Platform == PushNotificationChannelRecord.PlatformEnum.Wns);
        }

        private IQueryable<PushNotificationChannelRecord> GetAllRecipientsAsyncByTopic(string topic)
        {
            return _appDbContext.PushNotificationChannels
                .AsNoTracking()
                .Where(a => a.Topics.Any(t => t.Name == topic) && a.Platform == PushNotificationChannelRecord.PlatformEnum.Wns);
        }

        private IQueryable<PushNotificationChannelRecord> GetAllRecipientsAsyncByUid(string uid)
        {
            return _appDbContext.PushNotificationChannels
                .AsNoTracking()
                .Where(a => a.Uid == uid && a.Platform == PushNotificationChannelRecord.PlatformEnum.Wns);
        }


        private Task SendRawAsync(IEnumerable<PushNotificationChannelRecord> recipients, object rawContent)
        {
            return SendRawAsync(recipients, JsonSerializer.Serialize(rawContent));
        }

        private async Task SendRawAsync(IEnumerable<PushNotificationChannelRecord> recipients, string rawContent)
        {
            var recipientsList = recipients.ToList();
            if (recipientsList.Count == 0) return;

            var accessToken = await GetWnsAccessTokenAsync();

            foreach (var recipient in recipientsList)
            {
#pragma warning disable CS4014
                Task.Run(async () =>
                {
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
                        client.DefaultRequestHeaders.Add("X-WNS-TYPE", "wns/raw");

                        var payload = new ByteArrayContent(Encoding.UTF8.GetBytes(rawContent));

                        var result = await client.PostAsync(recipient.ChannelUri, payload);

                        if (!result.IsSuccessStatusCode)
                        {
                            var pushNotificationToDelete =
                                await _appDbContext.PushNotificationChannels.FirstOrDefaultAsync(entity =>
                                    entity.Id == recipient.Id);
                            if (pushNotificationToDelete != null)
                            {
                                _appDbContext.Remove(pushNotificationToDelete);
                                _appDbContext.SaveChangesAsync();
                            }
                        }
                    }
                });
#pragma warning restore CS4014
            }
        }

        private async Task<string> GetWnsAccessTokenAsync()
        {
            using var client = new HttpClient();
            var payload = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", _configuration.ClientId),
                new KeyValuePair<string, string>("client_secret", _configuration.ClientSecret),
                new KeyValuePair<string, string>("scope", "notify.windows.com")
            });

            var response = await client.PostAsync("https://login.live.com/accesstoken.srf", payload);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var accessTokenNode = JsonNode.Parse(content)!;

            return (string)accessTokenNode["access_token"] ?? "";
        }
    }
}