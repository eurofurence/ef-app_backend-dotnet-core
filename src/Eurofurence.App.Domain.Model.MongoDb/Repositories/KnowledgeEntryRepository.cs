using Eurofurence.App.Domain.Model.Knowledge;
using MongoDB.Driver;

namespace Eurofurence.App.Domain.Model.MongoDb.Repositories
{
    public class KnowledgeEntryRepository :
        MongoDbEntityRepositoryBase<KnowledgeEntryRecord>
    {
        public KnowledgeEntryRepository(IMongoCollection<KnowledgeEntryRecord> collection)
            : base(collection)
        {
        }
    }
}