using Eurofurence.App.Domain.Model.Announcements;
using MongoDB.Driver;

namespace Eurofurence.App.Domain.Model.MongoDb.Repositories
{
    public class AnnouncementRepository :
        MongoDbEntityRepositoryBase<AnnouncementRecord>
    {
        public AnnouncementRepository(IMongoCollection<AnnouncementRecord> collection)
            : base(collection)
        {
        }
    }
}