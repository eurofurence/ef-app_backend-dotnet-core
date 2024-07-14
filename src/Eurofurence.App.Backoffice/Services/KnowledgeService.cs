using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;
using Eurofurence.App.Domain.Model.Knowledge;

namespace Eurofurence.App.Backoffice.Services
{
    public class KnowledgeService(HttpClient http) : IKnowledgeService
    {
        public async Task<KnowledgeEntryRecord[]> GetKnowledgeEntriesAsync()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());
            return (await http.GetFromJsonAsync<KnowledgeEntryRecord[]>("knowledgeEntries", options))?.Where(ke => ke.IsDeleted != 1).ToArray() ?? [];
        }

        public async Task PutKnowledgeEntryAsync(Guid id, KnowledgeEntryRequest record)
        {
            JsonContent content = JsonContent.Create(record);
            await http.PutAsync($"knowledgeEntries/{id}", content);
        }

        public async Task PostKnowledgeEntryAsync(KnowledgeEntryRequest record)
        {
            JsonContent content = JsonContent.Create(record);
            await http.PostAsync($"knowledgeEntries", content);
        }

        public async Task DeleteKnowledgeEntryAsync(Guid id)
        {
            await http.DeleteAsync($"knowledgeEntries/{id}");
        }

        public async Task<KnowledgeGroupRecord[]> GetKnowledgeGroupsAsync()
        {
            return (await http.GetFromJsonAsync<KnowledgeGroupRecord[]>("knowledgeGroups"))?.Where(ke => ke.IsDeleted != 1).ToArray() ?? [];
        }
        public async Task PutKnowledgeGroupAsync(KnowledgeGroupRecord record)
        {
            JsonContent content = JsonContent.Create(record);
            await http.PutAsync($"knowledgeGroups/{record.Id}", content);
        }

        public async Task PostKnowledgeGroupAsync(KnowledgeGroupRecord record)
        {
            JsonContent content = JsonContent.Create(record);
            await http.PostAsync($"knowledgeGroups", content);
        }

        public async Task DeleteKnowledgeGroupAsync(Guid id)
        {
            await http.DeleteAsync($"knowledgeGroups/{id}");
        }
    }
}
