using Eurofurence.App.Domain.Model.Fursuits;
using Eurofurence.App.Domain.Model.MongoDb.Repositories;
using MongoDB.Driver;

namespace Eurofurence.App.Domain.Model.MongoDb.Fursuits
{
    public class FursuitBadgeImageRepository :
        MongoDbEntityRepositoryBase<FursuitBadgeImageRecord>
    {
        public FursuitBadgeImageRepository(IMongoCollection<FursuitBadgeImageRecord> collection)
            : base(collection)
        {
        }
    }
}