using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eurofurence.App.Domain.Model.Abstractions
{
    public interface IEntityRepository<TEntity> where TEntity: EntityBase
    {
        Task<TEntity> FindOneAsync(Guid id, bool includeDeletedRecords = false);
        Task<IEnumerable<TEntity>> FindAllAsync(bool includeDeletedRecords = false);
        Task ReplaceOneAsync(TEntity entity);
        Task InsertOneAsync(TEntity entity);
        Task DeleteOneAsync(Guid id);
    }


    public interface IEventRepository : IEntityRepository<EventRecord>
    {
        
    }

    public interface IEventConferenceTrackRepository : IEntityRepository<EventConferenceTrackRecord>
    {

    }

}
