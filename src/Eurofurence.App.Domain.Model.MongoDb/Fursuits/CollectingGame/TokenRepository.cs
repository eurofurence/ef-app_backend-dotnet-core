using Eurofurence.App.Domain.Model.Fursuits.CollectingGame;
using Eurofurence.App.Domain.Model.MongoDb.Repositories;
using MongoDB.Driver;

namespace Eurofurence.App.Domain.Model.MongoDb.Fursuits.CollectingGame
{
    public class TokenRepository :
        MongoDbEntityRepositoryBase<TokenRecord>
    {
        public TokenRepository(IMongoCollection<TokenRecord> collection)
            : base(collection)
        {
        }
    }
}