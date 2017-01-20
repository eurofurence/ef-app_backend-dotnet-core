using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public interface IEntityServiceOperations<TEntity>
    {
        Task<TEntity> FindOneAsync(Guid id);
        Task<IEnumerable<TEntity>> FindAllAsync(
            DateTime? minLastDateTimeChangedUtc = null
            );
        Task ReplaceOneAsync(TEntity entity);
        Task InsertOneAsync(TEntity entity);
        Task DeleteOneAsync(Guid id);
    }
}
