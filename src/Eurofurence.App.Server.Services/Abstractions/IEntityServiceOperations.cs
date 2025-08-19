using Eurofurence.App.Domain.Model;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public interface IEntityServiceOperations<TEntity, TResponse> :
        IEntityServiceReadOperations<TEntity>,
        IEntityServiceWriteOperations<TEntity>,
        IEntityServiceStorageOperations<TResponse>
        where TEntity : EntityBase
        where TResponse : ResponseBase
    {
    }
}