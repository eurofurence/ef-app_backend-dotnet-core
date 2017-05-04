using Eurofurence.App.Domain.Model.Events;
using MongoDB.Driver;

namespace Eurofurence.App.Domain.Model.MongoDb.Repositories
{
    public class EventFeedbackRepository :
    MongoDbEntityRepositoryBase<EventFeedbackRecord>
    {
        public EventFeedbackRepository(IMongoCollection<EventFeedbackRecord> collection)
            : base(collection)
        {

        }
    }
}