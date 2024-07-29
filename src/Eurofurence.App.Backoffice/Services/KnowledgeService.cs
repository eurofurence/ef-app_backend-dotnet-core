using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;
using Eurofurence.App.Domain.Model.Knowledge;

namespace Eurofurence.App.Backoffice.Services
{
    public class KnowledgeService(HttpClient http) : IKnowledgeService
    {
        public async Task<KnowledgeEntryResponse[]> GetKnowledgeEntriesAsync()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());
            return (await http.GetFromJsonAsync<KnowledgeEntryResponse[]>("knowledgeEntries", options))?.Where(ke => ke.IsDeleted != 1).ToArray() ?? [];
        }

        public async Task<bool> PutKnowledgeEntryAsync(Guid id, KnowledgeEntryRequest record)
        {
            JsonContent content = JsonContent.Create(record);
            var response = await http.PutAsync($"knowledgeEntries/{id}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> PostKnowledgeEntryAsync(KnowledgeEntryRequest record)
        {
            JsonContent content = JsonContent.Create(record);
            var response = await http.PostAsync($"knowledgeEntries", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteKnowledgeEntryAsync(Guid id)
        {
            var response = await http.DeleteAsync($"knowledgeEntries/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<KnowledgeGroupRecord[]> GetKnowledgeGroupsAsync()
        {
            return (await http.GetFromJsonAsync<KnowledgeGroupRecord[]>("knowledgeGroups"))?.Where(ke => ke.IsDeleted != 1).ToArray() ?? [];
        }
        public async Task<bool> PutKnowledgeGroupAsync(KnowledgeGroupRecord record)
        {
            JsonContent content = JsonContent.Create(record);
            var response = await http.PutAsync($"knowledgeGroups/{record.Id}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> PostKnowledgeGroupAsync(KnowledgeGroupRecord record)
        {
            JsonContent content = JsonContent.Create(record);
            var response = await http.PostAsync($"knowledgeGroups", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteKnowledgeGroupAsync(Guid id)
        {
            var response = await http.DeleteAsync($"knowledgeGroups/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
