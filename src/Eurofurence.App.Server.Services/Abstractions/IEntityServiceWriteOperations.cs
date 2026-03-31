using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eurofurence.App.Common.Abstractions;
using Eurofurence.App.Domain.Model;

namespace Eurofurence.App.Server.Services.Abstractions
{
    /// <summary>
    /// Provides write operations for entities.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    public interface IEntityServiceWriteOperations<TEntity> where TEntity : IEntityBase
    {
        /// <summary>
        /// Replaces a single entity of type <typeparamref name="TEntity"/> in the database.
        /// </summary>
        /// <param name="entity">The entity to replace.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>An empty task.</returns>
        Task ReplaceOneAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Replaces multiple entities of type <typeparamref name="TEntity"/> in the database.
        /// </summary>
        /// <param name="entities">The entities to replace.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>An empty task.</returns>
        Task ReplaceMultipleAsync(ICollection<TEntity> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// Inserts a single entity of type <typeparamref name="TEntity"/> into the database.
        /// </summary>
        /// <param name="entity">The entry to insert.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>An empty task.</returns>
        Task InsertOneAsync(TEntity entity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Inserts multiple entities of type <typeparamref name="TEntity"/> into the database.
        /// </summary>
        /// <param name="entities">The entities to insert.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>An empty task.</returns>
        Task InsertMultipleAsync(ICollection<TEntity> entities, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the entity with the given id.
        /// </summary>
        /// <param name="id">The id of the entry to delete.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>An empty task.</returns>
        Task DeleteOneAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes all elements in the given dbset <typeparamref name="TEntity"/> with the given ids.
        /// </summary>
        /// <param name="ids">The ids to delete.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>An empty task.</returns>
        Task DeleteMultipleAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = default);

        /// <summary>
        /// Hard deletes all elements in the given dbset <typeparamref name="TEntity"/>.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        Task DeleteAllAsync(CancellationToken cancellationToken = default);
    }
}