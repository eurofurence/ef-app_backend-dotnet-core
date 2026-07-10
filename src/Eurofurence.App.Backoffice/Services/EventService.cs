using System.Net;
using System.Net.Http.Json;
using Eurofurence.App.Domain.Model.Events;
using static System.Net.WebRequestMethods;

namespace Eurofurence.App.Backoffice.Services
{
    public class EventService(HttpClient http) : IEventService
    {
        public async Task<EventStatisticsResponse[]> GetEventStatisticsAsync()
        {
            return (await http.GetFromJsonAsync<EventStatisticsResponse[]>("events/statistics"))?.ToArray() ?? [];
        }

        public async Task<EventConferenceDayResponse[]> GetEventConferenceDaysAsync()
        {
            return (await http.GetFromJsonAsync<EventConferenceDayResponse[]>("eventConferenceDays"))?.ToArray() ?? [];
        }
    }
}
