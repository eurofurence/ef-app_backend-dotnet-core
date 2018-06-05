using Eurofurence.App.Domain.Model.MongoDb.Repositories;
using Eurofurence.App.Domain.Model.Security;
using MongoDB.Driver;

namespace Eurofurence.App.Domain.Model.MongoDb.Security
{
    public class RegSysAccessTokenRepository :
        MongoDbEntityRepositoryBase<RegSysAccessTokenRecord>
    {
        public RegSysAccessTokenRepository(IMongoCollection<RegSysAccessTokenRecord> collection)
            : base(collection)
        {
        }
    }
}