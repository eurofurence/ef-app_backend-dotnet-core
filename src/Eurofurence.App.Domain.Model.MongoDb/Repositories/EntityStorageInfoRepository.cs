using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Sync;
using MongoDB.Driver;

namespace Eurofurence.App.Domain.Model.MongoDb.Repositories
{
    public class EntityStorageInfoRepository : MongoDbEntityRepositoryBase<EntityStorageInfoRecord>,
        IEntityStorageInfoRepository
    {
        public EntityStorageInfoRepository(IMongoCollection<EntityStorageInfoRecord> collection)
            : base(collection)
        {

        }

        public async Task<EntityStorageInfoRecord> FindOneAsync(string entityType)
        {
            var results = await Collection.FindAsync(entity => entity.EntityType == entityType);
            return await results.FirstOrDefaultAsync();
        }
    }
}