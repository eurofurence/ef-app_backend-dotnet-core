using System.Net;
using System.Net.Http.Json;
using Eurofurence.App.Domain.Model.Events;
using static System.Net.WebRequestMethods;

namespace Eurofurence.App.Backoffice.Services
{
    public class EventService(HttpClient http) : IEventService
    {
        public async Task<EventResponse[]> GetEventsAsync()
        {
            return (await http.GetFromJsonAsync<EventResponse[]>("events"))?.ToArray() ?? [];
        }

        public async Task<EventConferenceDayResponse[]> GetEventConferenceDaysAsync()
        {
            return (await http.GetFromJsonAsync<EventConferenceDayResponse[]>("eventConferenceDays"))?.ToArray() ?? [];
        }
    }
}
