using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Domain.Model.ArtistsAlley;

namespace Eurofurence.App.Backoffice.Services
{
    public class AnnouncementService(HttpClient http) : IAnnouncementService
    {
        public async Task<IEnumerable<AnnouncementResponse>> GetAnnouncementsAsync()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());
            return (await http.GetFromJsonAsync<AnnouncementResponse[]>("Announcements", options))?.ToArray() ?? [];
        }
        public async Task SubmitAnnouncementAsync(AnnouncementRequest request)
        {
            JsonContent content = JsonContent.Create(request);
            await http.PostAsync($"Announcements", content);
        }
    }
}