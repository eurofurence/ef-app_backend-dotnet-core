using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model;
using Eurofurence.App.Domain.Model.Sync;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public interface IEntityServiceOperations<TEntity> where TEntity: EntityBase
    {
        Task<TEntity> FindOneAsync(Guid id);
        Task<IEnumerable<TEntity>> FindAllAsync();
        Task ReplaceOneAsync(TEntity entity);
        Task InsertOneAsync(TEntity entity);
        Task DeleteOneAsync(Guid id);
        Task DeleteAllAsync();

        Task<DeltaResponse<TEntity>> GetDeltaResponseAsync(DateTime? minLastDateTimeChangedUtc = null);

        Task<EntityStorageInfoRecord> GetStorageInfoAsync();
    }
}
