using Eurofurence.App.Domain.Model.Announcements;

namespace Eurofurence.App.Backoffice.Services
{
    public interface IAnnouncementService
    {
        Task<IEnumerable<AnnouncementResponse>> GetAnnouncementsAsync();

        Task SubmitAnnouncementAsync(AnnouncementRequest request);
    }
}