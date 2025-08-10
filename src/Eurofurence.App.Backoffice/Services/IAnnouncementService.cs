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
        /// Submits a new announcement by sending the provided request data to the server.
        /// </summary>
        /// <param name="request">The announcement details to be submitted, including required fields such as validity period, area, author, title, and content.</param>
        /// <return>A task representing the asynchronous operation.</return>
        Task SubmitAnnouncementAsync(AnnouncementRequest request);
    }
}
