using Eurofurence.App.Domain.Model.Knowledge;

namespace Eurofurence.App.Backoffice.Services
{
    public interface IKnowledgeService
    {
        public Task<KnowledgeEntryResponse[]> GetKnowledgeEntriesAsync();
        public Task<bool> PutKnowledgeEntryAsync(Guid id, KnowledgeEntryRequest record);
        public Task<bool> PostKnowledgeEntryAsync(KnowledgeEntryRequest record);
        public Task<bool> DeleteKnowledgeEntryAsync(Guid id);
        public Task<KnowledgeGroupRecord[]> GetKnowledgeGroupsAsync();
        public Task<bool> PutKnowledgeGroupAsync(KnowledgeGroupRecord record);
        public Task<bool> PostKnowledgeGroupAsync(KnowledgeGroupRecord record);
        public Task<bool> DeleteKnowledgeGroupAsync(Guid id);
    }
}
