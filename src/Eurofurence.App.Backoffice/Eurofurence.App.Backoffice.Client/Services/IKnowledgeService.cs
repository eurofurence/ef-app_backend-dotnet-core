using Eurofurence.App.Backoffice.Client.Models;

namespace Eurofurence.App.Backoffice.Client.Services
{
    public interface IKnowledgeService
    {
        public Task<KnowledgeEntry[]> GetKnowledgeEntriesAsync();
    }
}
