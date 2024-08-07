using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public interface IEntityServiceReadOperations<TEntity> where TEntity : EntityBase
    {
        Task<TEntity> FindOneAsync(Guid id, CancellationToken cancellationToken = default);
        IQueryable<TEntity> FindAll();
        IQueryable<TEntity> FindAll(Expression<Func<TEntity, bool>> filter);
        Task<bool> HasOneAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> HasManyAsync(params Guid?[] ids);
        Task<bool> HasManyAsync(IEnumerable<Guid?> ids, CancellationToken cancellationToken = default);
    }
}