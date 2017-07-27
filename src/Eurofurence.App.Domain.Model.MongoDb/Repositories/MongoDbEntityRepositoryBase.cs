using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Abstractions;
using MongoDB.Driver;

namespace Eurofurence.App.Domain.Model.MongoDb.Repositories
{
    public class MongoDbEntityRepositoryBase<TEntity> : IEntityRepository<TEntity> where TEntity : EntityBase
    {
        protected readonly IMongoCollection<TEntity> Collection;

        public MongoDbEntityRepositoryBase(IMongoCollection<TEntity> collection)
        {
            Collection = collection;
        }

        public virtual async Task<TEntity> FindOneAsync(Guid id, bool includeDeletedRecords = false)
        {
            var results = await Collection.FindAsync(entity =>
                entity.Id == id && (includeDeletedRecords || entity.IsDeleted == 0)
            );
            return await results.FirstOrDefaultAsync();
        }

        public virtual async Task<TEntity> FindOneAsync(Expression<Func<TEntity, bool>> filter)
        {
            var results = await Collection.FindAsync(new FilterDefinitionBuilder<TEntity>().Where(filter));
            return await results.FirstOrDefaultAsync();
        }

        public virtual async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<Guid> ids, bool includeDeletedRecords = false)
        {
            var idList = ids.ToList();
            var filters = new List<FilterDefinition<TEntity>>();

            if (!includeDeletedRecords)
                filters.Add(new FilterDefinitionBuilder<TEntity>()
                    .Eq(a => a.IsDeleted, 0));

            filters.Add(new FilterDefinitionBuilder<TEntity>()
                .Where(entity => idList.Contains(entity.Id)));

            var results = await Collection.FindAsync(new FilterDefinitionBuilder<TEntity>().And(filters));
            return await results.ToListAsync();
        }

        public virtual async Task<IEnumerable<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> filter)
        {
            var results = await Collection.FindAsync(new FilterDefinitionBuilder<TEntity>().Where(filter));
            return await results.ToListAsync();
        }

        public virtual async Task<IEnumerable<TEntity>> FindAllAsync(
            bool includeDeletedRecords = false,
            DateTime? minLastDateTimeChangedUtc = null)
        {
            var filters = new List<FilterDefinition<TEntity>>();

            if (!includeDeletedRecords)
                filters.Add(new FilterDefinitionBuilder<TEntity>()
                    .Eq(a => a.IsDeleted, 0));

            if (minLastDateTimeChangedUtc.HasValue)
                filters.Add(new FilterDefinitionBuilder<TEntity>()
                    .Gte(a => a.LastChangeDateTimeUtc, minLastDateTimeChangedUtc.Value));

            var results = await Collection.FindAsync(new FilterDefinitionBuilder<TEntity>().And(filters));
            return await results.ToListAsync();
        }

        public virtual Task ReplaceOneAsync(TEntity entity)
        {
            return Collection.ReplaceOneAsync(existingEntity => existingEntity.Id == entity.Id, entity);
        }

        public virtual Task InsertOneAsync(TEntity entity)
        {
            return Collection.InsertOneAsync(entity);
        }

        public virtual Task DeleteOneAsync(Guid id)
        {
            return Collection.DeleteOneAsync(entity => entity.Id == id);
        }

        public virtual Task DeleteAllAsync()
        {
            return Collection.DeleteManyAsync(entity => true);
        }
    }
}