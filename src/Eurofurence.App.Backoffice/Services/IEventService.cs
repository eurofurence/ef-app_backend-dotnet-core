using Eurofurence.App.Domain.Model.Events;

namespace Eurofurence.App.Backoffice.Services
{
    public interface IEventService
    {
        public Task<EventWithStatisticsResponse[]> GetEventsWithStatisticsAsync();

        public Task<EventConferenceDayResponse[]> GetEventConferenceDaysAsync();
    }
}
