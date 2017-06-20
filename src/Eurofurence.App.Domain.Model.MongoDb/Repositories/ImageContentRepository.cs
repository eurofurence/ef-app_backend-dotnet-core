using Eurofurence.App.Domain.Model.Images;
using MongoDB.Driver;

namespace Eurofurence.App.Domain.Model.MongoDb.Repositories
{
    public class ImageContentRepository :
        MongoDbEntityRepositoryBase<ImageContentRecord>
    {
        public ImageContentRepository(IMongoCollection<ImageContentRecord> collection)
            : base(collection)
        {
        }
    }
}