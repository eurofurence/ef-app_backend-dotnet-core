using Eurofurence.App.Domain.Model.Events;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions.Events
{
    public interface IEventFavoriteStatisticsService :
        IEntityServiceOperations<EventFavoriteStatisticsRecord, EventFavoriteStatisticsResponse>,
        IPatchOperationProcessor<EventFavoriteStatisticsRecord>
    {
        /// <summary>
        /// Inserts EventFavoriteStatisticsRecords for all events that have started and do not yet have a corresponding EventFavoriteStatisticsRecord.
        /// </summary>
        public Task InsertForAllStartedEvents();

        /// <summary>
        /// Computes the EventFavoriteStatistics for a given EventRecord and returns a list of EventFavoriteStatisticsRecords.
        /// </summary>
        /// <param name="eventRecord">
        /// The event record for which to compute favorite statistics.
        /// Must include the FavoredBy collection to accurately compute the statistics.
        /// </param>
        /// <returns>
        /// A list of event favorite statistics records for the given event.
        /// </returns>
        public List<EventFavoriteStatisticsRecord> ComputeEventFavoriteStatistics(EventRecord eventRecord);
    }
}
