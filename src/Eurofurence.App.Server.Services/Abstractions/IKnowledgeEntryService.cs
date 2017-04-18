using Eurofurence.App.Domain.Model.Knowledge;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public interface IKnowledgeEntryService :
    IEntityServiceOperations<KnowledgeEntryRecord>,
    IPatchOperationProcessor<KnowledgeEntryRecord>
    {

    }
}