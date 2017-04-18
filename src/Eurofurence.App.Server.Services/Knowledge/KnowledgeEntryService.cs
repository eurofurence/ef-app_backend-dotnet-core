using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Knowledge;
using Eurofurence.App.Server.Services.Abstractions;

namespace Eurofurence.App.Server.Services.Events
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