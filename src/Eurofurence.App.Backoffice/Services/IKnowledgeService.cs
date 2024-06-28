using Eurofurence.App.Backoffice.Models;

namespace Eurofurence.App.Backoffice.Services
{
    public interface IKnowledgeService
    {
        public Task<KnowledgeEntry[]> GetKnowledgeEntriesAsync();
    }
}
