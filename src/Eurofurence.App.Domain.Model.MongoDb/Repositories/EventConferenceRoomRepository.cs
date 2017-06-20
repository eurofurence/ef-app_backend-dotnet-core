using Eurofurence.App.Domain.Model.Events;
using MongoDB.Driver;

namespace Eurofurence.App.Domain.Model.MongoDb.Repositories
{
    public class EventConferenceRoomRepository :
        MongoDbEntityRepositoryBase<EventConferenceRoomRecord>
    {
        public EventConferenceRoomRepository(IMongoCollection<EventConferenceRoomRecord> collection)
            : base(collection)
        {
        }
    }
}