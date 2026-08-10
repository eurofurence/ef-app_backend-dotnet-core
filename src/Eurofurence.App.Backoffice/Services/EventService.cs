using Eurofurence.App.Domain.Model.Events;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Eurofurence.App.Backoffice.Services
{
    public class EventService(HttpClient http) : IEventService
    {
        /// <inheritdoc />
        public async Task<EventResponse[]> GetEventsAsync()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());
            return (await http.GetFromJsonAsync<EventResponse[]>("events", options))?.ToArray() ?? [];
        }

        /// <inheritdoc />
        public async Task<EventWithStatisticsResponse[]> GetEventsWithStatisticsAsync()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());
            return (await http.GetFromJsonAsync<EventWithStatisticsResponse[]>("events/statistics", options))?.ToArray() ?? [];
        }

        /// <inheritdoc />
        public async Task<EventConferenceDayResponse[]> GetEventConferenceDaysAsync()
        {
            return (await http.GetFromJsonAsync<EventConferenceDayResponse[]>("eventConferenceDays"))?.ToArray() ?? [];
        }

        /// <inheritdoc />
        public async Task UpdateEventBannerImageAsync(Guid id, Guid? imageId)
        {
            JsonContent? content = imageId is not null ? JsonContent.Create(imageId) : null;
            using var response = await http.PutAsync($"Events/{id}/:bannerImageId", content);
            response.EnsureSuccessStatusCode();
        }

        /// <inheritdoc />
        public async Task UpdateEventPosterImageAsync(Guid id, Guid? imageId)
        {
            JsonContent? content = imageId is not null ? JsonContent.Create(imageId) : null;
            using var response = await http.PutAsync($"Events/{id}/:posterImageId", content);
            response.EnsureSuccessStatusCode();
        }
    }
}
