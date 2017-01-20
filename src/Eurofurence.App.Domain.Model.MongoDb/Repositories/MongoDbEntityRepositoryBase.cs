using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Abstractions;
using MongoDB.Driver;

namespace Eurofurence.App.Domain.Model.MongoDb.Repositories
{
    public class MongoDbEntityRepositoryBase<TEntity> : IEntityRepository<TEntity> where TEntity : EntityBase
    {
        private readonly IMongoCollection<TEntity> _collection;

        public MongoDbEntityRepositoryBase(IMongoCollection<TEntity> collection)
        {
            _collection = collection;
        }

        public virtual async Task<TEntity> FindOneAsync(Guid id, bool includeDeletedRecords = false)
        {
            var results = await _collection.FindAsync(entity => 
                entity.Id == id && (includeDeletedRecords || entity.IsDeleted == 0)
            );
            return await results.FirstOrDefaultAsync();
        }

        public virtual async Task<IEnumerable<TEntity>> FindAllAsync(bool includeDeletedRecords = false)
        {
            var results = await _collection.FindAsync(entity => (includeDeletedRecords || entity.IsDeleted == 0));
            return await results.ToListAsync();
        }

        public virtual Task ReplaceOneAsync(TEntity entity)
        {
            return _collection.ReplaceOneAsync(existingEntity => existingEntity.Id == entity.Id, entity);
        }

        public virtual Task InsertOneAsync(TEntity entity)
        {
            return _collection.InsertOneAsync(entity);
        }

        public virtual Task DeleteOneAsync(Guid id)
        {
            return _collection.DeleteOneAsync(entity => entity.Id == id);
        }
    }
}