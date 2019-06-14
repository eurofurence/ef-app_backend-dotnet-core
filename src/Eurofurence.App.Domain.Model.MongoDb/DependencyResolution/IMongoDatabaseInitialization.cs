using MongoDB.Driver;

namespace Eurofurence.App.Domain.Model.MongoDb.DependencyResolution
{
    public interface IMongoDatabaseInitialization
    {
        void ExecuteInitializationTasks(IMongoDatabase database);
    }
}