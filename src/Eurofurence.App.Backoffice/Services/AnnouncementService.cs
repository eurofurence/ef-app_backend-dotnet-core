using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Eurofurence.App.Domain.Model.Announcements;

namespace Eurofurence.App.Backoffice.Services
{
    public class AnnouncementService(HttpClient http) : IAnnouncementService
    {
        /// <inheritdoc />
        public async Task<IEnumerable<AnnouncementResponse>> GetAnnouncementsAsync()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());
            return (await http.GetFromJsonAsync<AnnouncementResponse[]>("Announcements/:all", options))?.ToArray() ?? [];
        }

        /// <inheritdoc />
        public async Task UpdateAnnouncementAsync(Guid id, AnnouncementRequest request)
        {
            JsonContent content = JsonContent.Create(request);
            using var response = await http.PutAsync($"Announcements/{id}", content);
            response.EnsureSuccessStatusCode();
        }

        /// <inheritdoc />
        public async Task DeleteAnnouncementAsync(Guid id)
        {
            using var response = await http.DeleteAsync($"Announcements/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}