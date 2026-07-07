using Eurofurence.App.Domain.Model.Events;

namespace Eurofurence.App.Backoffice.Services
{
    public interface IEventService
    {
        public Task<EventResponse[]> GetEventsAsync();

        public Task<EventConferenceDayResponse[]> GetEventConferenceDaysAsync();
    }
}
