using Eurofurence.App.Domain.Model.Knowledge;

namespace Eurofurence.App.Backoffice.Services
{
    public interface IKnowledgeService
    {
        public Task<KnowledgeEntryRecord[]> GetKnowledgeEntriesAsync();
    }
}
