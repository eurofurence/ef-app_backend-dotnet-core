using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Announcements;

namespace Eurofurence.App.Server.Services.Abstractions.PushNotifications
{
    public interface IPushEventMediator
    {
        Task PushSyncRequestAsync();

        Task PushAnnouncementNotificationAsync(AnnouncementRecord announcement);
    }
}