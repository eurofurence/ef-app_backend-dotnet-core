using Eurofurence.App.Domain.Model.Events;
using MongoDB.Driver;

namespace Eurofurence.App.Domain.Model.MongoDb.Repositories
{
    public class EventConferenceDayRepository :
        MongoDbEntityRepositoryBase<EventConferenceDayRecord>
    {
        public EventConferenceDayRepository(IMongoCollection<EventConferenceDayRecord> collection)
            : base(collection)
        {
        }
    }
}