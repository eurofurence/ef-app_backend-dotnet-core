using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Knowledge;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Knowledge;

namespace Eurofurence.App.Server.Services.Knowledge
{
    public class KnowledgeEntryService : EntityServiceBase<KnowledgeEntryRecord>,
        IKnowledgeEntryService
    {
        public KnowledgeEntryService(
            IEntityRepository<KnowledgeEntryRecord> entityRepository,
            IStorageServiceFactory storageServiceFactory
            )
            : base(entityRepository, storageServiceFactory)
        {
        }
    }
}