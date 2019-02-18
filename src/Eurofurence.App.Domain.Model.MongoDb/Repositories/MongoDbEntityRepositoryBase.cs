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

        SortDefinition<TEntity> BuildSortDefinitionFromFilterOptions(FilterOptions<TEntity> filterOptions)
        {
            if (filterOptions?.SortFields == null) return null;

            var builder = new SortDefinitionBuilder<TEntity>();
            SortDefinition<TEntity> sortDefinition = null;

            foreach (var sortField in filterOptions.SortFields)
            {
                switch (sortField.Order)
                {
                    case FilterOptions<TEntity>.SortOrderEnum.Ascending:
                        sortDefinition = sortDefinition?.Ascending(sortField.Field)
                            ?? builder.Ascending(sortField.Field);
                        break;
                    case FilterOptions<TEntity>.SortOrderEnum.Descending:
                        sortDefinition = sortDefinition?.Descending(sortField.Field)
                            ?? builder.Descending(sortField.Field);
                        break;
                }
            }

            return sortDefinition;
        }

        FindOptions<TEntity, TEntity> BuildFindOptionsFromFilterOptions(FilterOptions<TEntity> filterOptions)
        {
            if (filterOptions == null) return null;

            var findOptions = new FindOptions<TEntity, TEntity>();
            findOptions.Sort = BuildSortDefinitionFromFilterOptions(filterOptions);
            findOptions.Limit = filterOptions.MaxRecordCount;

            return findOptions;
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

        public virtual async Task<IEnumerable<TEntity>> FindAllAsync(
            IEnumerable<Guid> ids, 
            bool includeDeletedRecords = false,
            FilterOptions<TEntity> filterOptions = null
        )
        {
            var idList = ids.ToList();
            var filters = new List<FilterDefinition<TEntity>>();

            if (!includeDeletedRecords)
                filters.Add(new FilterDefinitionBuilder<TEntity>()
                    .Eq(a => a.IsDeleted, 0));

            filters.Add(new FilterDefinitionBuilder<TEntity>()
                .Where(entity => idList.Contains(entity.Id)));

            var results = await Collection.FindAsync(
                new FilterDefinitionBuilder<TEntity>().And(filters),
                BuildFindOptionsFromFilterOptions(filterOptions)
            );

            return await results.ToListAsync();
        }

        public virtual async Task<IEnumerable<TEntity>> FindAllAsync(
            Expression<Func<TEntity, bool>> filter,
            FilterOptions<TEntity> filterOptions = null
        )
        {
            var results = await Collection.FindAsync(
                new FilterDefinitionBuilder<TEntity>().Where(filter),
                BuildFindOptionsFromFilterOptions(filterOptions)
            );

            return await results.ToListAsync();
        }

        public virtual async Task<IEnumerable<TEntity>> FindAllAsync(
            bool includeDeletedRecords = false,
            DateTime? minLastDateTimeChangedUtc = null,
            FilterOptions<TEntity> filterOptions = null
        )
        {
            var filters = new List<FilterDefinition<TEntity>>();

            if (!includeDeletedRecords)
                filters.Add(new FilterDefinitionBuilder<TEntity>()
                    .Eq(a => a.IsDeleted, 0));

            if (minLastDateTimeChangedUtc.HasValue)
                filters.Add(new FilterDefinitionBuilder<TEntity>()
                    .Gte(a => a.LastChangeDateTimeUtc, minLastDateTimeChangedUtc.Value));

            var results = await Collection.FindAsync(
                new FilterDefinitionBuilder<TEntity>().And(filters),
                BuildFindOptionsFromFilterOptions(filterOptions)
            );
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

        public virtual async Task<bool> HasAsync(Guid id, bool includeDeletedRecords = false)
        {
            var count = await Collection.CountAsync(entity => entity.Id == id && (entity.IsDeleted == 0 || includeDeletedRecords));
            return count > 0;
        }

        public virtual async Task<bool> HasManyAsync(Guid[] ids, bool includeDeletedRecords = false)
        {
            var count = await Collection.CountAsync(
                entity => ids.Contains(entity.Id) && (entity.IsDeleted == 0 || includeDeletedRecords));

            return count == ids.Length;
        }
    }
}