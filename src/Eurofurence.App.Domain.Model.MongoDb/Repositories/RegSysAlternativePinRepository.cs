using Eurofurence.App.Domain.Model.Security;
using MongoDB.Driver;

namespace Eurofurence.App.Domain.Model.MongoDb.Repositories
{
    public class RegSysAlternativePinRepository :
        MongoDbEntityRepositoryBase<RegSysAlternativePinRecord>
    {
        public RegSysAlternativePinRepository(IMongoCollection<RegSysAlternativePinRecord> collection)
            : base(collection)
        {
        }
    }
}