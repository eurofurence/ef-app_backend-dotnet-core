using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Domain.Model.Sync;

namespace Eurofurence.App.Domain.Model.Abstractions
{
    public interface IEntityRepository<TEntity> where TEntity : EntityBase
    {
        Task<TEntity> FindOneAsync(Guid id, bool includeDeletedRecords = false);
        Task<TEntity> FindOneAsync(Expression<Func<TEntity, bool>> filter);

        Task<IEnumerable<TEntity>> FindAllAsync(
            bool includeDeletedRecords = false,
            DateTime? minLastDateTimeChangedUtc = null,
            FilterOptions<TEntity> filterOptions = null
        );

        Task<IEnumerable<TEntity>> FindAllAsync(
            IEnumerable<Guid> ids,
            bool includeDeletedRecords = false,
            FilterOptions<TEntity> filterOptions = null
        );

        Task<IEnumerable<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> filter, FilterOptions<TEntity> filterOptions = null);
        Task ReplaceOneAsync(TEntity entity);
        Task InsertOneAsync(TEntity entity);
        Task DeleteOneAsync(Guid id);
        Task DeleteAllAsync();
    }


    public interface IEventRepository : IEntityRepository<EventRecord>
    {
    }

    public interface IEventConferenceTrackRepository : IEntityRepository<EventConferenceTrackRecord>
    {
    }

    public interface IEntityStorageInfoRepository : IEntityRepository<EntityStorageInfoRecord>
    {
        Task<EntityStorageInfoRecord> FindOneAsync(string entityType);
    }
}