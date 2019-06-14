using MongoDB.Driver;

namespace Eurofurence.App.Domain.Model.MongoDb.DependencyResolution
{
    public interface IMongoDatabaseBroker
    {
        void Setup(IMongoDatabase database);
        IMongoCollection<TRecord> GetCollection<TRecord>(string name);
    }
}