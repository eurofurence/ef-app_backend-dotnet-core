using Eurofurence.App.Domain.Model.Announcements;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public interface IPushEventMediator
    {
        Task PushSyncRequestAsync();

        Task PushAnnouncementNotificationAsync(AnnouncementRecord announcement);
    }
}