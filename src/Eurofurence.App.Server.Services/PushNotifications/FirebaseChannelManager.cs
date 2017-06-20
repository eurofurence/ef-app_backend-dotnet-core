using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;
using Newtonsoft.Json;

namespace Eurofurence.App.Server.Services.PushNotifications
{
    public class FirebaseChannelManager : IFirebaseChannelManager
    {
        private readonly FirebaseConfiguration _configuration;

        public FirebaseChannelManager(FirebaseConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task PushAnnouncementNotificationAsync(AnnouncementRecord announcement)
        {
            return SendPushNotificationAsync(new
            {
                Event = "Announcement",
                announcement.Title,
                Text = announcement.Content
            });
        }

        public Task PushSyncRequestAsync()
        {
            return SendPushNotificationAsync(new
            {
                Event = "Sync"
            });
        }

        private async Task SendPushNotificationAsync(object data)
        {
            var payload = new
            {
                to = $"/topics/{_configuration.TargetTopic}",
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
    }
}