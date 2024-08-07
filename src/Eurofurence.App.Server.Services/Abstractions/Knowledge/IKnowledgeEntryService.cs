using Eurofurence.App.Domain.Model.Knowledge;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions.Knowledge
{
    public interface IKnowledgeEntryService :
        IEntityServiceOperations<KnowledgeEntryRecord>,
        IPatchOperationProcessor<KnowledgeEntryRecord>
    {
        public Task<KnowledgeEntryRecord> InsertKnowledgeEntryAsync(
            KnowledgeEntryRequest request,
            CancellationToken cancellationToken = default);

        public Task<KnowledgeEntryRecord> ReplaceKnowledgeEntryAsync(
            Guid id,
            KnowledgeEntryRequest request,
            CancellationToken cancellationToken = default);
    }
}