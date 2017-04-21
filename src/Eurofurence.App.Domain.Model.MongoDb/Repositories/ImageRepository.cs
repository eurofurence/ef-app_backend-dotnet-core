using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Domain.Model.Images;
using MongoDB.Driver;

namespace Eurofurence.App.Domain.Model.MongoDb.Repositories
{
    public class ImageRepository :
        MongoDbEntityRepositoryBase<ImageRecord>
    {
        public ImageRepository(IMongoCollection<ImageRecord> collection)
            : base(collection)
        {

        }
    }
}