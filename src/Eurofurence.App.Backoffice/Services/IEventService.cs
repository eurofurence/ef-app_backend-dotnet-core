using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Domain.Model.Events;

namespace Eurofurence.App.Backoffice.Services
{
    public interface IEventService
    {
        /// <summary>
        /// Retrieves a list of events with statistics from the server.
        /// </summary>
        /// <return>A collection of events with statistics.</return>
        public Task<EventWithStatisticsResponse[]> GetEventsWithStatisticsAsync();

        /// <summary>
        /// Retrieves a list of event conference days from the server.
        /// </summary>
        /// <return>A collection of event conference days.</return>
        public Task<EventConferenceDayResponse[]> GetEventConferenceDaysAsync();

        /// <summary>
        /// Updates the banner image of an existing event with the specified image.
        /// </summary>
        /// <param name="id">The id of the event to be updated.</param>
        /// <param name="imageId">The id of the image to be used as the banner.</param>
        /// <return>An empty Task.</return>
        Task UpdateEventBannerImageAsync(Guid id, Guid? imageId);

        /// <summary>
        /// Updates the poster image of an existing event with the specified image.
        /// </summary>
        /// <param name="id">The id of the event to be updated.</param>
        /// <param name="imageId">The id of the image to be used as the poster.</param>
        /// <return>An empty Task.</return>
        Task UpdateEventPosterImageAsync(Guid id, Guid? imageId);
    }
}
