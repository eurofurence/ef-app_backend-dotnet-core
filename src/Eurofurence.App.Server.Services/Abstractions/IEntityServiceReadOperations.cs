using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public interface IEntityServiceReadOperations<TEntity> where TEntity : EntityBase
    {
        Task<TEntity> FindOneAsync(Guid id);
        Task<IEnumerable<TEntity>> FindAllAsync();
        Task<IEnumerable<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> filter);
        Task<bool> HasOneAsync(Guid id);
        Task<bool> HasManyAsync(params Guid?[] ids);
    }
}