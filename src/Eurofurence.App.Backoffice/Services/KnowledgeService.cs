using System.Net.Http.Json;
using Eurofurence.App.Domain.Model.Knowledge;

namespace Eurofurence.App.Backoffice.Services
{
    public class KnowledgeService(HttpClient http) : IKnowledgeService
    {
        public async Task<KnowledgeEntryRecord[]> GetKnowledgeEntriesAsync()
        {
            // Make the API call
            return await http.GetFromJsonAsync<KnowledgeEntryRecord[]>("knowledgeEntries") ?? [];
        }
    }
}
