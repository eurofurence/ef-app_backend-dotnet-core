using Eurofurence.App.Domain.Model.ArtShow;
using Eurofurence.App.Domain.Model.MongoDb.Repositories;
using MongoDB.Driver;

namespace Eurofurence.App.Domain.Model.MongoDb.ArtShow
{
    public class ItemActivityRepository :
        MongoDbEntityRepositoryBase<ItemActivityRecord>
    {
        public ItemActivityRepository(IMongoCollection<ItemActivityRecord> collection)
            : base(collection)
        {
        }
    }
}