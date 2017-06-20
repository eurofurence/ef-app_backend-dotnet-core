using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Domain.Model.PushNotifications;
using Eurofurence.App.Server.Services.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;

namespace Eurofurence.App.Server.Services.PushNotifications
{
    public class WnsConfiguration
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string TargetTopic { get; set; }
    }

    public class WnsChannelManager : IWnsChannelManager
    {
        readonly IEntityRepository<PushNotificationChannelRecord> _pushNotificationRepository;
        readonly WnsConfiguration _configuration;

        public WnsChannelManager(
            IEntityRepository<PushNotificationChannelRecord> pushNotificationRepository,
            WnsConfiguration configuraton
            )
        {
            _configuration = configuraton;
            _pushNotificationRepository = pushNotificationRepository;
        }

        public async Task PushSyncRequestAsync()
        {
            var recipients = await GetAllRecipientsAsyncByTopic(_configuration.TargetTopic);
            await SendRawAsync(recipients, "update");
        }

        private Task<IEnumerable<PushNotificationChannelRecord>> GetAllRecipientsAsync()
        {
            return _pushNotificationRepository.FindAllAsync();
        }

        private Task<IEnumerable<PushNotificationChannelRecord>> GetAllRecipientsAsyncByTopic(string topic)
        {
            return _pushNotificationRepository.FindAllAsync(a => a.Topics.Contains(topic));
        }
        private Task<IEnumerable<PushNotificationChannelRecord>> GetAllRecipientsAsyncByUid(string uid)
        {
            return _pushNotificationRepository.FindAllAsync(a => a.Uid == uid);
        }

        public async Task PushAnnouncementNotificationAsync(AnnouncementRecord announcement)
        {
            var message = new
            {
                Event = "NewAnnouncement",
                Content = announcement
            };

            var recipients = await GetAllRecipientsAsyncByTopic(_configuration.TargetTopic);
            
            await  SendRawAsync(recipients, Newtonsoft.Json.JsonConvert.SerializeObject(message));
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
                            _pushNotificationRepository.DeleteOneAsync(recipient.Id);
                        }
                    }
                });
                #pragma warning restore CS4014
            }

        }


        private async Task SendToastAsync(IEnumerable<PushNotificationChannelRecord> recipients, string title, string message)
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
                        client.DefaultRequestHeaders.Add("X-WNS-TYPE", "wns/toast");

                        var xml = $"<?xml version=\"1.0\" encoding=\"utf-8\"?><toast><visual><binding template=\"ToastText01\"><text id=\"1\">{message}</text></binding></visual></toast>";
                        var payload = new StringContent(xml, Encoding.UTF8, "text/xml");

                        var result = await client.PostAsync(recipient.ChannelUri, payload);

                        if (!result.IsSuccessStatusCode)
                        {
                            _pushNotificationRepository.DeleteOneAsync(recipient.Id);
                        }
                    }
                });
                #pragma warning restore CS4014
            }

        }

        public async Task SendToastAsync(string topic, string message)
        {
            var recipients = await GetAllRecipientsAsyncByTopic(topic);
            var accessToken = await GetWnsAccessTokenAsync();

            foreach(var recipient in recipients)
            {
                #pragma warning disable CS4014
                Task.Run(async () =>
                {
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
                        client.DefaultRequestHeaders.Add("X-WNS-TYPE", "wns/toast");

                        var xml = $"<?xml version=\"1.0\" encoding=\"utf-8\"?><toast><visual><binding template=\"ToastText01\"><text id=\"1\">{message}</text></binding></visual></toast>";
                        var payload = new StringContent(xml, Encoding.UTF8, "text/xml");

                        var result = await client.PostAsync(recipient.ChannelUri, payload);

                        if (!result.IsSuccessStatusCode)
                        {
                            _pushNotificationRepository.DeleteOneAsync(recipient.Id);
                        }
                    }
                });
                #pragma warning restore CS4014
            }

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

        async Task<string> GetWnsAccessTokenAsync()
        {
            using (var client = new HttpClient())
            {
                var payload = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
                    new KeyValuePair<string, string>("client_id", _configuration.ClientId),
                    new KeyValuePair<string, string>("client_secret", _configuration.ClientSecret),
                    new KeyValuePair<string, string>("scope", "notify.windows.com"),
                });

                var response = await client.PostAsync("https://login.live.com/accesstoken.srf", payload);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var schema = new { token_type = "", access_token = "", expires_in = 0 };
                var node = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(content, schema);

                return node.access_token;
            }
        }

        public async Task PushPrivateMessageNotificationAsync(string recipientUid)
        {
            var recipients = await GetAllRecipientsAsyncByUid(recipientUid);
            await SendRawAsync(recipients, "privatemessage_received");
        }
    }
}
