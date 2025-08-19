using Eurofurence.App.Domain.Model.Knowledge;

namespace Eurofurence.App.Backoffice.Services
{
    public interface IKnowledgeService
    {
        public Task<KnowledgeEntryResponse[]> GetKnowledgeEntriesAsync();
        public Task<bool> PutKnowledgeEntryAsync(Guid id, KnowledgeEntryRequest record);
        public Task<bool> PostKnowledgeEntryAsync(KnowledgeEntryRequest record);
        public Task<bool> DeleteKnowledgeEntryAsync(Guid id);
        public Task<KnowledgeGroupResponse[]> GetKnowledgeGroupsAsync();
        public Task<bool> PutKnowledgeGroupAsync(Guid id, KnowledgeGroupRequest record);
        public Task<bool> PostKnowledgeGroupAsync(KnowledgeGroupRequest record);
        public Task<bool> DeleteKnowledgeGroupAsync(Guid id);
    }
}
