using Eurofurence.App.Domain.Model;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public interface IEntityServiceOperations<TEntity> :
        IEntityServiceReadOperations<TEntity>,
        IEntityServiceWriteOperations<TEntity>,
        IEntityServiceStorageOperations<TEntity>
        where TEntity : EntityBase
    {
    }
}