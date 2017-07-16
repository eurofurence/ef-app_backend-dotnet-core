using Eurofurence.App.Domain.Model.Fursuits;
using Eurofurence.App.Domain.Model.MongoDb.Repositories;
using MongoDB.Driver;

namespace Eurofurence.App.Domain.Model.MongoDb.Fursuits
{
    public class FursuitBadgeRepository :
        MongoDbEntityRepositoryBase<FursuitBadgeRecord>
    {
        public FursuitBadgeRepository(IMongoCollection<FursuitBadgeRecord> collection)
            : base(collection)
        {
        }
    }
}