using Eurofurence.App.Domain.Model.Knowledge;

namespace Eurofurence.App.Backoffice.Services
{
    public interface IKnowledgeService
    {
        public Task<KnowledgeEntryRecord[]> GetKnowledgeEntriesAsync();
        public Task PutKnowledgeEntryAsync(Guid id, KnowledgeEntryRequest record);
        public Task PostKnowledgeEntryAsync(KnowledgeEntryRequest record);
        public Task DeleteKnowledgeEntryAsync(Guid id);
        public Task<KnowledgeGroupRecord[]> GetKnowledgeGroupsAsync();
        public Task PutKnowledgeGroupAsync(KnowledgeGroupRecord record);
        public Task PostKnowledgeGroupAsync(KnowledgeGroupRecord record);
        public Task DeleteKnowledgeGroupAsync(Guid id);
    }
}
