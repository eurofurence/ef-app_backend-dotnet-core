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

namespace Eurofurence.App.Server.Services.PushNotifications
{
    public class WnsConfiguration
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
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

        public Task PushSyncUpdateRequestAsync(string topic)
        {
            return SendRawAsync(topic, "update");
        }

        public Task PushAnnouncementAsync(string topic, AnnouncementRecord announcement)
        {
            var message = new
            {
                Event = "NewAnnouncement",
                Content = announcement
            };

            return SendRawAsync(topic, Newtonsoft.Json.JsonConvert.SerializeObject(message));
        }


        private async Task SendRawAsync(string topic, string rawContent)
        {
            var recipients = await _pushNotificationRepository.FindAllAsync(a => a.Topics.Contains(topic));
            var accessToken = await GetWnsAccessTokenAsync();

            foreach (var recipient in recipients)
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

        public async Task SendToastAsync(string topic, string message)
        {
            var recipients = await _pushNotificationRepository.FindAllAsync(a => a.Topics.Contains(topic));
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

    }
}
