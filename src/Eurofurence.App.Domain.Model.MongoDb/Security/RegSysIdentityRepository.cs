using Eurofurence.App.Domain.Model.MongoDb.Repositories;
using Eurofurence.App.Domain.Model.Security;
using MongoDB.Driver;

namespace Eurofurence.App.Domain.Model.MongoDb.Security
{
    public class RegSysIdentityRepository :
        MongoDbEntityRepositoryBase<RegSysIdentityRecord>
    {
        public RegSysIdentityRepository(IMongoCollection<RegSysIdentityRecord> collection)
            : base(collection)
        {
        }
    }
}