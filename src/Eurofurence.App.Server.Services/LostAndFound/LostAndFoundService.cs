using Eurofurence.App.Domain.Model.LostAndFound;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.LostAndFound;

namespace Eurofurence.App.Server.Services.LostAndFound
{
    public class LostAndFoundService : EntityServiceBase<LostAndFoundRecord, LostAndFoundResponse>, ILostAndFoundService
    {
        public LostAndFoundService(
            AppDbContext appDbContext,
           IStorageServiceFactory storageServiceFactory
       )
           : base(appDbContext, storageServiceFactory)
        {
        }
    }
}
