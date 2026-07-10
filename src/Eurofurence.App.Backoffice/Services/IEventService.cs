using Eurofurence.App.Domain.Model.Events;

namespace Eurofurence.App.Backoffice.Services
{
    public interface IEventService
    {
        public Task<EventStatisticsResponse[]> GetEventStatisticsAsync();

        public Task<EventConferenceDayResponse[]> GetEventConferenceDaysAsync();
    }
}
