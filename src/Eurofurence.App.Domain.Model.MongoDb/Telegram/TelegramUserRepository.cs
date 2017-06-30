using Eurofurence.App.Domain.Model.MongoDb.Repositories;
using Eurofurence.App.Domain.Model.Telegram;
using MongoDB.Driver;

namespace Eurofurence.App.Domain.Model.MongoDb.Telegram
{
    public class TelegramUserRepository :
        MongoDbEntityRepositoryBase<TelegramUserRecord>
    {
        public TelegramUserRepository(IMongoCollection<TelegramUserRecord> collection)
            : base(collection)
        {
        }
    }
}