using Eurofurence.App.Domain.Model.Events;
using MongoDB.Driver;

namespace Eurofurence.App.Domain.Model.MongoDb.Repositories
{
    public class EventRepository :
        MongoDbEntityRepositoryBase<EventRecord>
    {
        public EventRepository(IMongoCollection<EventRecord> collection)
            : base(collection)
        {

        }
    }
}