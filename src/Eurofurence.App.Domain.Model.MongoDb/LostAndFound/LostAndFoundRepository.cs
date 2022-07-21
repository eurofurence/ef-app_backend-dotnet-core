using Eurofurence.App.Domain.Model.LostAndFound;
using Eurofurence.App.Domain.Model.MongoDb.Repositories;
using MongoDB.Driver;

namespace Eurofurence.App.Domain.Model.MongoDb.LostAndFound
{
    public class LostAndFoundRepository : MongoDbEntityRepositoryBase<LostAndFoundRecord>
    {
        public LostAndFoundRepository(IMongoCollection<LostAndFoundRecord> collection)
            : base(collection)
        {
        }
    }
}