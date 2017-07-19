using Eurofurence.App.Domain.Model.MongoDb.Repositories;
using Eurofurence.App.Domain.Model.Telegram;
using MongoDB.Driver;

namespace Eurofurence.App.Domain.Model.MongoDb.Telegram
{
    public class UserRepository :
        MongoDbEntityRepositoryBase<UserRecord>
    {
        public UserRepository(IMongoCollection<UserRecord> collection)
            : base(collection)
        {
        }
    }
}