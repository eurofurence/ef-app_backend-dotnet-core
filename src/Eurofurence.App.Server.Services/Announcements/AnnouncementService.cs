using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Announcements;

namespace Eurofurence.App.Server.Services.Announcements
{
    public class AnnouncementService : EntityServiceBase<AnnouncementRecord>,
        IAnnouncementService
    {
        public AnnouncementService(
            IEntityRepository<AnnouncementRecord> entityRepository,
            IStorageServiceFactory storageServiceFactory
        )
            : base(entityRepository, storageServiceFactory)
        {
        }
    }
}