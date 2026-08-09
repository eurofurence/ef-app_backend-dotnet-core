using Eurofurence.App.Domain.Model.Announcements;

namespace Eurofurence.App.Backoffice.Services
{
    /// <summary>
    /// Provides methods for managing and retrieving announcements from the server.
    /// </summary>
    public interface IAnnouncementService
    {
        /// <summary>
        /// Retrieves a list of announcements from the server.
        /// </summary>
        /// <return>A collection of announcements.</return>
        Task<IEnumerable<AnnouncementResponse>> GetAnnouncementsAsync();

        /// <summary>
        /// Updates an existing announcement with the specified data.
        /// </summary>
        /// <param name="id">The id of the announcement to be updated.</param>
        /// <param name="request">The request object containing the updated announcement details.</param>
        /// <return>An empty Task.</return>
        Task UpdateAnnouncementAsync(Guid id, AnnouncementRequest request);

        /// <summary>
        /// Deletes an announcement with the specified id.
        /// </summary>
        /// <param name="id">The id of the announcement to be deleted</param>
        /// <returns>An empty Task.</returns>
        Task DeleteAnnouncementAsync(Guid id);
    }
}
