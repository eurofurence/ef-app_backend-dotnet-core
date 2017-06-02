using Eurofurence.App.Domain.Model.Communication;
using MongoDB.Driver;

namespace Eurofurence.App.Domain.Model.MongoDb.Repositories
{
    public class PrivateMessageRepository :
    MongoDbEntityRepositoryBase<PrivateMessageRecord>
    {
        public PrivateMessageRepository(IMongoCollection<PrivateMessageRecord> collection)
            : base(collection)
        {

        }
    }
}