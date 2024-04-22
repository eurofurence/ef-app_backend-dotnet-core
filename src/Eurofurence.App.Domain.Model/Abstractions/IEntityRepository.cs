using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Domain.Model.Sync;

namespace Eurofurence.App.Domain.Model.Abstractions
{
    //public interface IEntityRepository<TEntity> where TEntity : EntityBase
    //{
    //    Task<TEntity> FindOneAsync(Expression<Func<TEntity, bool>> where, bool includeDeletedRecords = false);

    //    Task<TEntity> FindOneByIdAsync(Guid id, bool includeDeletedRecords = false);
        
    //    bool Has(Guid id, bool includeDeletedRecords = false);

    //    bool HasMany(Guid[] ids, bool includeDeletedRecords = false);

    //    IQueryable<TEntity> FindAll(
    //        IEnumerable<Guid> ids = null,
    //        bool includeDeletedRecords = false,
    //        DateTime? minLastDateTimeChangedUtc = null
    //    );

    //    Task ReplaceOneAsync(TEntity entity);

    //    Task InsertOneAsync(TEntity entity);

    //    Task DeleteOneAsync(Guid id);

    //    Task DeleteAllAsync();

    //}
}