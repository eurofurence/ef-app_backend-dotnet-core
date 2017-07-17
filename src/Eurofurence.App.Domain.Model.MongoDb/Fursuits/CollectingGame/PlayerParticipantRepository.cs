using Eurofurence.App.Domain.Model.Fursuits.CollectingGame;
using Eurofurence.App.Domain.Model.MongoDb.Repositories;
using MongoDB.Driver;

namespace Eurofurence.App.Domain.Model.MongoDb.Fursuits.CollectingGame
{
    public class PlayerParticipantRepository :
        MongoDbEntityRepositoryBase<PlayerParticipationRecord>
    {
        public PlayerParticipantRepository(IMongoCollection<PlayerParticipationRecord> collection)
            : base(collection)
        {
        }
    }
}