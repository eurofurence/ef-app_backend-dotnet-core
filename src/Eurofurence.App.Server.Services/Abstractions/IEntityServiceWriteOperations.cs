using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public interface IEntityServiceWriteOperations<TEntity> where TEntity : EntityBase
    {
        Task ReplaceOneAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task ReplaceMultipleAsync(ICollection<TEntity> entities, CancellationToken cancellationToken = default);
        Task InsertOneAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task InsertMultipleAsync(ICollection<TEntity> entities, CancellationToken cancellationToken = default);
        Task DeleteOneAsync(Guid id, CancellationToken cancellationToken = default);
        Task DeleteMultipleAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);
        Task DeleteAllAsync(CancellationToken cancellationToken = default);
    }
}