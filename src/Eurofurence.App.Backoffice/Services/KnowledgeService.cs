using System.Net.Http.Json;
using Eurofurence.App.Backoffice.Models;

namespace Eurofurence.App.Backoffice.Services
{
    public class KnowledgeService(HttpClient http) : IKnowledgeService
    {
        public async Task<KnowledgeEntry[]> GetKnowledgeEntriesAsync()
        {
            // Make the API call
            return await http.GetFromJsonAsync<KnowledgeEntry[]>("knowledgeEntries") ?? [];
        }
    }
}
