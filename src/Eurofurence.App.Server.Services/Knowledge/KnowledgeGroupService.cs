using Eurofurence.App.Domain.Model.Knowledge;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Knowledge;

namespace Eurofurence.App.Server.Services.Knowledge
{
    public class KnowledgeGroupService : EntityServiceBase<KnowledgeGroupRecord>,
        IKnowledgeGroupService
    {
        public KnowledgeGroupService(
            AppDbContext appDbContext,
            IStorageServiceFactory storageServiceFactory
        )
            : base(appDbContext, storageServiceFactory)
        {
        }
    }
}