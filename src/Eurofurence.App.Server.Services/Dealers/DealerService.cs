using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Dealers;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Dealers;

namespace Eurofurence.App.Server.Services.Dealers
{
    public class DealerService : EntityServiceBase<DealerRecord>,
        IDealerService
    {
        public DealerService(
            IEntityRepository<DealerRecord> entityRepository,
            IStorageServiceFactory storageServiceFactory
        )
            : base(entityRepository, storageServiceFactory)
        {
        }
    }
}