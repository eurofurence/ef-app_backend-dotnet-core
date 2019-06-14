using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace Eurofurence.App.Domain.Model.MongoDb.DependencyResolution
{
    internal class MongoDatabaseInitialization : IMongoDatabaseInitialization
    {
        public List<Action<IMongoDatabase>> InitializationTasks { get; }

        public MongoDatabaseInitialization()
        {
            InitializationTasks = new List<Action<IMongoDatabase>>();
        }

        public void ExecuteInitializationTasks(IMongoDatabase database)
        {
            InitializationTasks.ForEach(task => task.Invoke(database));
        }
    }
}