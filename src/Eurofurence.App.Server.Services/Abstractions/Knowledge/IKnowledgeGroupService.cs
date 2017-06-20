using Eurofurence.App.Domain.Model.Knowledge;

namespace Eurofurence.App.Server.Services.Abstractions.Knowledge
{
    public interface IKnowledgeGroupService :
        IEntityServiceOperations<KnowledgeGroupRecord>,
        IPatchOperationProcessor<KnowledgeGroupRecord>
    {
    }
}