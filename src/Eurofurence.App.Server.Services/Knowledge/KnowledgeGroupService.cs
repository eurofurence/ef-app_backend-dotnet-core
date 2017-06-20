using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Knowledge;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Knowledge;

namespace Eurofurence.App.Server.Services.Knowledge
{
    public class KnowledgeGroupService : EntityServiceBase<KnowledgeGroupRecord>,
        IKnowledgeGroupService
    {
        public KnowledgeGroupService(
            IEntityRepository<KnowledgeGroupRecord> entityRepository,
            IStorageServiceFactory storageServiceFactory
            )
            : base(entityRepository, storageServiceFactory)
        {
        }
    }
}