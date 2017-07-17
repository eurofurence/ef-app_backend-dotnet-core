using Eurofurence.App.Domain.Model.Fursuits.CollectingGame;
using Eurofurence.App.Domain.Model.MongoDb.Repositories;
using MongoDB.Driver;

namespace Eurofurence.App.Domain.Model.MongoDb.Fursuits.CollectingGame
{
    public class FursuitParticipantRepository :
        MongoDbEntityRepositoryBase<FursuitParticipationRecord>
    {
        public FursuitParticipantRepository(IMongoCollection<FursuitParticipationRecord> collection)
            : base(collection)
        {
        }
    }
}