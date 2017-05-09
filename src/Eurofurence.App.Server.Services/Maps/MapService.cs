using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Maps;
using Eurofurence.App.Server.Services.Abstractions;

namespace Eurofurence.App.Server.Services.Maps
{
    public class MapService : EntityServiceBase<MapRecord>,
        IMapService
    {
        public MapService(
            IEntityRepository<MapRecord> entityRepository,
            IStorageServiceFactory storageServiceFactory
            )
            : base(entityRepository, storageServiceFactory)
        {
        }
    }
}
