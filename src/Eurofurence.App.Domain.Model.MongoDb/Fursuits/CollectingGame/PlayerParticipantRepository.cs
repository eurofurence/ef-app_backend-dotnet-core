using Eurofurence.App.Domain.Model.Fursuits.CollectingGame;
using Eurofurence.App.Domain.Model.MongoDb.Repositories;
using MongoDB.Driver;

namespace Eurofurence.App.Domain.Model.MongoDb.Fursuits.CollectingGame
{
    public class PlayerParticipantRepository :
        MongoDbEntityRepositoryBase<PlayerParticipantRecord>
    {
        public PlayerParticipantRepository(IMongoCollection<PlayerParticipantRecord> collection)
            : base(collection)
        {
        }
    }
}