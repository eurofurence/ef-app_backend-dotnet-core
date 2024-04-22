using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Announcements;

namespace Eurofurence.App.Server.Services.Announcements
{
    public class AnnouncementService : EntityServiceBase<AnnouncementRecord>,
        IAnnouncementService
    {
        public AnnouncementService(
            AppDbContext appDbContext,
            IStorageServiceFactory storageServiceFactory
        )
            : base(appDbContext, storageServiceFactory)
        {
        }
    }
}