using Eurofurence.App.Domain.Model;
using Eurofurence.App.Domain.Model.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Eurofurence.App.Tests.Common.InMemoryRepository
{
    public class InMemoryEntityRepository<T> : IEntityRepository<T> where T : EntityBase
    {
         private List<T> _entities = new List<T>();

        public Task DeleteAllAsync()
        {
            _entities.Clear();
            return Task.CompletedTask;
        }

        public Task DeleteOneAsync(Guid id)
        {
            _entities.RemoveAll(a => a.Id == id);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<T>> FindAllAsync(bool includeDeletedRecords = false, DateTime? minLastDateTimeChangedUtc = null, FilterOptions<T> filterOptions = null)
        {
            var query = _entities.AsQueryable();

            if (!includeDeletedRecords)
                query = query.Where(a => a.IsDeleted == 0);

            if (minLastDateTimeChangedUtc.HasValue)
                query = query.Where(a => a.LastChangeDateTimeUtc > minLastDateTimeChangedUtc.Value);

            query = ApplyFilterOptions(query, filterOptions);

            return new TaskFactory<IEnumerable<T>>().StartNew(() => query.ToList());
        }

        public Task<IEnumerable<T>> FindAllAsync(IEnumerable<Guid> ids, bool includeDeletedRecords = false, FilterOptions<T> filterOptions = null)
        {
            var query = _entities.AsQueryable();

            query = query.Where(entity => ids.Contains(entity.Id));

            if (!includeDeletedRecords)
                query = query.Where(a => a.IsDeleted == 0);

            query = ApplyFilterOptions(query, filterOptions);

            return new TaskFactory<IEnumerable<T>>().StartNew(() => query.ToList());
        }

        public Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> filter, FilterOptions<T> filterOptions = null)
        {
            var query = _entities.AsQueryable();
            var f = filter.Compile();

            query = query.Where(entity => f(entity));
            query = ApplyFilterOptions(query, filterOptions);

            return new TaskFactory<IEnumerable<T>>().StartNew(() => query.ToList());
        }

        public Task<T> FindOneAsync(Guid id, bool includeDeletedRecords = false)
        {
            return new TaskFactory<T>().StartNew(() =>
                _entities.FirstOrDefault(
                    entity => entity.Id == id && (includeDeletedRecords || entity.IsDeleted == 0)));
        }

        public Task<T> FindOneAsync(Expression<Func<T, bool>> filter)
        {
            var f = filter.Compile();
            return new TaskFactory<T>().StartNew(() =>
                _entities.Where(entity => f(entity)).FirstOrDefault());
        }

        public Task<bool> HasAsync(Guid id, bool includeDeletedRecords = false)
        {
            throw new NotImplementedException();
        }

        public Task<bool> HasManyAsync(Guid[] ids, bool includeDeletedRecords = false)
        {
            throw new NotImplementedException();
        }

        public Task InsertOneAsync(T entity)
        {
            _entities.Add(entity);
            return Task.CompletedTask;
        }

        public Task ReplaceOneAsync(T entity)
        {
            _entities.RemoveAll(a => a.Id == entity.Id);
            _entities.Add(entity);

            return Task.CompletedTask;
        }

        private IQueryable<T> ApplyFilterOptions<T>(IQueryable<T> input, FilterOptions<T> filterOptions)
        {
            if (filterOptions == null) return input;

            IOrderedQueryable<T> inputSorted = null;

            foreach(var sort in filterOptions.SortFields)
            {
                switch (sort.Order)
                {
                    case FilterOptions<T>.SortOrderEnum.Ascending:
                        inputSorted = inputSorted?.ThenBy(sort.Field) ?? input.OrderBy(sort.Field);
                        break;
                    case FilterOptions<T>.SortOrderEnum.Descending:
                        inputSorted = inputSorted?.ThenByDescending(sort.Field) ?? input.OrderByDescending(sort.Field);
                        break;
                }
            }

            input = inputSorted ?? input;

            if (filterOptions.MaxRecordCount.HasValue)
                input = input.Take(filterOptions.MaxRecordCount.Value);

            return input;
        }
    }
}
