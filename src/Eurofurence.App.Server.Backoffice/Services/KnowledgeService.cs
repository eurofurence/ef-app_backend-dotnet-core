using Eurofurence.App.Server.Backoffice.Models;

namespace Eurofurence.App.Server.Backoffice.Services
{
    public class KnowledgeService(HttpClient http) : IKnowledgeService
    {
        public async Task<KnowledgeEntry[]> GetKnowledgeEntriesAsync()
        {
            return await http.GetFromJsonAsync<KnowledgeEntry[]>("knowledgeEntries") ?? [];
        }
    }
}
