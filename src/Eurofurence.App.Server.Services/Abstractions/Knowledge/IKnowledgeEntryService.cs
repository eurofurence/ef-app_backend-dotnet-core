using Eurofurence.App.Domain.Model.Knowledge;
using System;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions.Knowledge
{
    public interface IKnowledgeEntryService :
        IEntityServiceOperations<KnowledgeEntryRecord>,
        IPatchOperationProcessor<KnowledgeEntryRecord>
    {
        public Task<KnowledgeEntryRecord> InsertKnowledgeEntryAsync(KnowledgeEntryRequest request);

        public Task<KnowledgeEntryRecord> ReplaceKnowledgeEntryAsync(Guid id, KnowledgeEntryRequest request);
    }
}