using Eurofurence.App.Server.Backoffice.Models;

namespace Eurofurence.App.Server.Backoffice.Services
{
    public interface IKnowledgeService
    {
        public Task<KnowledgeEntry[]> GetKnowledgeEntriesAsync();
    }
}
