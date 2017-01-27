using Eurofurence.App.Domain.Model.Events;
using MongoDB.Driver;

namespace Eurofurence.App.Domain.Model.MongoDb.Repositories
{
    public class EventConferenceTrackRepository : 
        MongoDbEntityRepositoryBase<EventConferenceTrackRecord>
    {
        public EventConferenceTrackRepository(IMongoCollection<EventConferenceTrackRecord> collection)
            : base(collection)
        {
            
        }
    }
}
