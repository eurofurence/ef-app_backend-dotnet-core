using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.LostAndFound;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.LostAndFound;

namespace Eurofurence.App.Server.Services.LostAndFound
{
    public class LostAndFoundService : EntityServiceBase<LostAndFoundRecord>, ILostAndFoundService
    {
        public LostAndFoundService(
           IEntityRepository<LostAndFoundRecord> entityRepository,
           IStorageServiceFactory storageServiceFactory
       )
           : base(entityRepository, storageServiceFactory)
        {
        }
    }
}
