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
        /// <summary>
        /// Submits a new announcement by sending the provided request data to the server.
        /// </summary>
        /// <param name="request">The announcement details to be submitted, including required fields such as validity period, area, author, title, and content.</param>
        /// <return>A task representing the asynchronous operation.</return>
        public async Task SubmitAnnouncementAsync(AnnouncementRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            JsonContent content = JsonContent.Create(request);
            using var response = await http.PostAsync($"Announcements", content);
            response.EnsureSuccessStatusCode();
        }
    }
}