using Eurofurence.App.Domain.Model.Events;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Eurofurence.App.Backoffice.Services
{
    public class EventService(HttpClient http) : IEventService
    {
        public async Task<EventWithStatisticsResponse[]> GetEventsWithStatisticsAsync()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());
            return (await http.GetFromJsonAsync<EventWithStatisticsResponse[]>("events/statistics", options))?.ToArray() ?? [];
        }

        public async Task<EventConferenceDayResponse[]> GetEventConferenceDaysAsync()
        {
            return (await http.GetFromJsonAsync<EventConferenceDayResponse[]>("eventConferenceDays"))?.ToArray() ?? [];
        }
    }
}
