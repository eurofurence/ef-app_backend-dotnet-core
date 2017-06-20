using Eurofurence.App.Domain.Model.Knowledge;
using MongoDB.Driver;

namespace Eurofurence.App.Domain.Model.MongoDb.Repositories
{
    public class KnowledgeGroupRepository :
        MongoDbEntityRepositoryBase<KnowledgeGroupRecord>
    {
        public KnowledgeGroupRepository(IMongoCollection<KnowledgeGroupRecord> collection)
            : base(collection)
        {
        }
    }
}