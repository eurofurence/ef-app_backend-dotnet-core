using Eurofurence.App.Domain.Model.Maps;
using MongoDB.Driver;

namespace Eurofurence.App.Domain.Model.MongoDb.Repositories
{
    public class MapRepository :
    MongoDbEntityRepositoryBase<MapRecord>
    {
        public MapRepository(IMongoCollection<MapRecord> collection)
            : base(collection)
        {

        }
    }
}