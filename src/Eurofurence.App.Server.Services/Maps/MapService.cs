using Eurofurence.App.Domain.Model.Maps;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Maps;

namespace Eurofurence.App.Server.Services.Maps
{
    public class MapService : EntityServiceBase<MapRecord>,
        IMapService
    {
        public MapService(
            AppDbContext appDbContext,
            IStorageServiceFactory storageServiceFactory
        )
            : base(appDbContext, storageServiceFactory)
        {
        }
    }
}