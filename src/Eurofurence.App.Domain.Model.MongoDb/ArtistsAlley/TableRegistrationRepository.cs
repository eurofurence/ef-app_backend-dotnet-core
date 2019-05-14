using Eurofurence.App.Domain.Model.ArtistsAlley;
using Eurofurence.App.Domain.Model.MongoDb.Repositories;
using MongoDB.Driver;

namespace Eurofurence.App.Domain.Model.MongoDb.ArtShow
{
    public class TableRegistrationRepository :
        MongoDbEntityRepositoryBase<TableRegistrationRecord>
    {
        public TableRegistrationRepository(IMongoCollection<TableRegistrationRecord> collection)
            : base(collection)
        {
        }
    }
}