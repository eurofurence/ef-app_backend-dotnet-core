using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Domain.Model.Sync;
using System.Linq.Expressions;

namespace Eurofurence.App.Domain.Model.Abstractions
{
    public interface IEntityRepository<TEntity> where TEntity: EntityBase
    {
        Task<TEntity> FindOneAsync(Guid id, bool includeDeletedRecords = false);
        Task<IEnumerable<TEntity>> FindAllAsync(
            bool includeDeletedRecords = false, 
            DateTime? minLastDateTimeChangedUtc = null);
        Task<IEnumerable<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> filter);
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
