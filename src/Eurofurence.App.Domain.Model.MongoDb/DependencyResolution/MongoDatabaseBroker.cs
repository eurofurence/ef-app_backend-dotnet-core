using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace Eurofurence.App.Domain.Model.MongoDb.DependencyResolution
{
    internal class MongoDatabaseBroker : IMongoDatabaseBroker
    {
        public List<Action<IMongoDatabase>> InitializationTasks { get; }

        private IMongoDatabase _database;

        public MongoDatabaseBroker()
        {
            InitializationTasks = new List<Action<IMongoDatabase>>();
        }

        public void Setup(IMongoDatabase database)
        {
            _database = database;
            InitializationTasks.ForEach(task => task.Invoke(_database));
        }

        public IMongoCollection<TRecord> GetCollection<TRecord>(string name)
        {
            return _database.GetCollection<TRecord>(name);
        }
    }
}