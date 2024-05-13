using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public interface IEntityServiceWriteOperations<TEntity> where TEntity : EntityBase
    {
        Task ReplaceOneAsync(TEntity entity);
        Task ReplaceMultipleAsync(IQueryable<TEntity> entities);
        Task InsertOneAsync(TEntity entity);
        Task InsertMultipleAsync(IQueryable<TEntity> entities);
        Task DeleteOneAsync(Guid id);
        Task DeleteMultipleAsync(IEnumerable<Guid> ids);
        Task DeleteAllAsync();
    }
}