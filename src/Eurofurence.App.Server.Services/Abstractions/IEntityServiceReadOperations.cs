using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Eurofurence.App.Common.Abstractions;

namespace Eurofurence.App.Server.Services.Abstractions
{
    /// <summary>
    /// Provides read-only operations for a specific entity type.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public interface IEntityServiceReadOperations<TEntity> where TEntity : IEntityBase
    {
        /// <summary>
        /// Retrieves a single entity by its unique identifier.
        ///
        /// Maybe null if not found.
        /// </summary>
        /// <param name="id">The id of the entity</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A task containing the entity if found.</returns>
        Task<TEntity> FindOneAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all entities of type <typeparamref name="TEntity"/>.
        /// </summary>
        /// <returns>A collection of all entities</returns>
        IQueryable<TEntity> FindAll();

        /// <summary>
        /// Retrieves all entities of type <typeparamref name="TEntity"/> that satisfy the specified filter expression.
        /// </summary>
        /// <param name="filter">The filter expression to use</param>
        /// <returns></returns>
        IQueryable<TEntity> FindAll(Expression<Func<TEntity, bool>> filter);

        /// <summary>
        /// Checks if an entity exists under <paramref name="id"/>.
        /// </summary>
        /// <param name="id">The id to check for existence.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>A task containing the result as a bool.</returns>
        Task<bool> HasOneAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if a collection of entities exists under <paramref name="ids"/>.
        /// </summary>
        /// <param name="ids">The collection of ids to check.</param>
        /// <returns>A task with the result as a bool.</returns>
        Task<bool> HasManyAsync(params Guid?[] ids);

        /// <summary>
        /// Checks if a collection of entities exists under <paramref name="ids"/>.
        /// </summary>
        /// <param name="ids">The collection of ids to check.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>A task with the result as a bool.</returns>
        Task<bool> HasManyAsync(IEnumerable<Guid?> ids, CancellationToken cancellationToken = default);
    }
}