using System;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public interface IEntityServiceWriteOperations<TEntity> where TEntity : EntityBase
    {
        Task ReplaceOneAsync(TEntity entity);
        Task InsertOneAsync(TEntity entity);
        Task DeleteOneAsync(Guid id);
        Task DeleteAllAsync();
    }
}
