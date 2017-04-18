using Eurofurence.App.Domain.Model.Knowledge;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public interface IKnowledgeGroupService :
    IEntityServiceOperations<KnowledgeGroupRecord>,
    IPatchOperationProcessor<KnowledgeGroupRecord>
    {

    }
}