using Eurofurence.App.Domain.Model.ArtShow;
using Eurofurence.App.Domain.Model.MongoDb.Repositories;
using MongoDB.Driver;

namespace Eurofurence.App.Domain.Model.MongoDb.ArtShow
{
    public class AgentClosingResultRepository :
        MongoDbEntityRepositoryBase<AgentClosingResultRecord>
    {
        public AgentClosingResultRepository(IMongoCollection<AgentClosingResultRecord> collection)
            : base(collection)
        {
        }
    }
}