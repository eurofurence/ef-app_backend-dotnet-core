using Eurofurence.App.Domain.Model.Knowledge;

namespace Eurofurence.App.Server.Services.Abstractions.Knowledge
{
    public interface IKnowledgeEntryService :
        IEntityServiceOperations<KnowledgeEntryRecord>,
        IPatchOperationProcessor<KnowledgeEntryRecord>
    {
    }
}