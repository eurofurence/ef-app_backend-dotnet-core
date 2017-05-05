using Eurofurence.App.Domain.Model.PushNotifications;
using MongoDB.Driver;

namespace Eurofurence.App.Domain.Model.MongoDb.Repositories
{
    public class PushNotificationChannelRepository :
    MongoDbEntityRepositoryBase<PushNotificationChannelRecord>
    {
        public PushNotificationChannelRepository(IMongoCollection<PushNotificationChannelRecord> collection)
            : base(collection)
        {

        }
    }
}