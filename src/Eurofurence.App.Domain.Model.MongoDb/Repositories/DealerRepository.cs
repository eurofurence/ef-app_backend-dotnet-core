using Eurofurence.App.Domain.Model.Dealers;
using MongoDB.Driver;

namespace Eurofurence.App.Domain.Model.MongoDb.Repositories
{
    public class DealerRepository :
    MongoDbEntityRepositoryBase<DealerRecord>
    {
        public DealerRepository(IMongoCollection<DealerRecord> collection)
            : base(collection)
        {

        }
    }
}