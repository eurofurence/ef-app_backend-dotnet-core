using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Eurofurence.App.Common.DataDiffUtils;
using Eurofurence.App.Domain.Model;
using Eurofurence.App.Domain.Model.Sync;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Eurofurence.App.Server.Services
{
    public class EntityServiceBase<T> :
        IEntityServiceOperations<T>,
        IPatchOperationProcessor<T>
        where T : EntityBase
    {
        private readonly AppDbContext _appDbContext;
        private readonly IStorageService _storageService;
        private readonly bool _useSoftDelete;

        public EntityServiceBase(
            AppDbContext appDbContext,
            IStorageServiceFactory storageServiceFactory,
            bool useSoftDelete = true)
        {
            _appDbContext = appDbContext;
            _storageService = storageServiceFactory.CreateStorageService<T>();
            _useSoftDelete = useSoftDelete;
        }

        public virtual async Task<T> FindOneAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Set<T>()
                .AsNoTracking()
                .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        }

        public virtual IQueryable<T> FindAll()
        {
            return _appDbContext.Set<T>().AsNoTracking();
        }

        public IQueryable<T> FindAll(Expression<Func<T, bool>> filter)
        {
            return _appDbContext.Set<T>().Where(filter).AsNoTracking();
        }

        public virtual async Task ReplaceOneAsync(T entity, CancellationToken cancellationToken = default)
        {
            entity.Touch();
            _appDbContext.Set<T>().Update(entity);
            await _storageService.TouchAsync(cancellationToken);
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task ReplaceMultipleAsync(
            ICollection<T> entities,
            CancellationToken cancellationToken = default)
        {
            if (entities == null || entities.Count == 0)
            {
                return;
            }

            foreach (var entity in entities)
            {
                entity.Touch();
            }

            _appDbContext.UpdateRange(entities);
            await _storageService.TouchAsync(cancellationToken);
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task InsertOneAsync(T entity, CancellationToken cancellationToken = default)
        {
            entity.Touch();
            _appDbContext.Add(entity);
            await _storageService.TouchAsync(cancellationToken);
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task InsertMultipleAsync(
            ICollection<T> entities,
            CancellationToken cancellationToken = default)
        {
            foreach (var entity in entities)
            {
                entity.Touch();
            }

            await _appDbContext.AddRangeAsync(entities, cancellationToken);
            await _storageService.TouchAsync(cancellationToken);
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task DeleteOneAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _appDbContext.Set<T>()
                .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

            if (_useSoftDelete)
            {
                entity.IsDeleted = 1;
                entity.Touch();
            }
            else
            {
                _appDbContext.Remove(entity);
            }

            await _storageService.TouchAsync(cancellationToken);
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task DeleteMultipleAsync(
            IEnumerable<Guid> ids,
            CancellationToken cancellationToken = default)
        {
            var entities = _appDbContext.Set<T>().Where(entity => ids.Contains(entity.Id));

            if (_useSoftDelete)
            {
                foreach (var entity in entities)
                {
                    entity.IsDeleted = 1;
                    entity.Touch();
                }
            }
            else
            {
                _appDbContext.Remove(entities);
            }

            await _storageService.TouchAsync(cancellationToken);
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual async Task DeleteAllAsync(CancellationToken cancellationToken = default)
        {
            _appDbContext.RemoveRange(_appDbContext.Set<T>());
            await _storageService.ResetDeltaStartAsync(cancellationToken);
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual Task<EntityStorageInfoRecord> GetStorageInfoAsync(CancellationToken cancellationToken = default)
        {
            return _storageService.GetStorageInfoAsync(cancellationToken);
        }

        public virtual async Task<DeltaResponse<T>> GetDeltaResponseAsync(
            DateTime? minLastDateTimeChangedUtc = null,
            CancellationToken cancellationToken = default)
        {
            var storageInfo = await GetStorageInfoAsync(cancellationToken);
            var response = new DeltaResponse<T>
            {
                StorageDeltaStartChangeDateTimeUtc = storageInfo.DeltaStartDateTimeUtc,
                StorageLastChangeDateTimeUtc = storageInfo.LastChangeDateTimeUtc
            };

            if (!minLastDateTimeChangedUtc.HasValue || minLastDateTimeChangedUtc < storageInfo.DeltaStartDateTimeUtc)
            {
                response.RemoveAllBeforeInsert = true;
                response.DeletedEntities = Array.Empty<Guid>();
                response.ChangedEntities = await
                    _appDbContext.Set<T>().Where(entity => entity.IsDeleted == 0).ToArrayAsync(cancellationToken);
            }
            else
            {
                response.RemoveAllBeforeInsert = false;

                var entities = _appDbContext.Set<T>().IgnoreQueryFilters().Where(entity => entity.LastChangeDateTimeUtc > minLastDateTimeChangedUtc);

                response.ChangedEntities = await entities
                    .Where(a => a.IsDeleted == 0)
                    .ToArrayAsync(cancellationToken);
                response.DeletedEntities = await entities
                    .Where(a => a.IsDeleted == 1)
                    .Select(a => a.Id)
                    .ToArrayAsync(cancellationToken);
            }

            return response;
        }

        public virtual async Task ApplyPatchOperationAsync(
            IEnumerable<PatchOperation<T>> patchResults,
            CancellationToken cancellationToken = default)
        {
            foreach (var item in patchResults)
                switch (item.Action)
                {
                    case ActionEnum.Add:
                        await InsertOneAsync(item.Entity, cancellationToken);
                        break;
                    case ActionEnum.Update:
                        await ReplaceOneAsync(item.Entity, cancellationToken);
                        break;
                    case ActionEnum.Delete:
                        await DeleteOneAsync(item.Entity.Id, cancellationToken);
                        break;
                }
        }

        public virtual async Task ResetStorageDeltaAsync(CancellationToken cancellationToken = default)
        {
            var items = _appDbContext.Set<T>()
                .Where(entity => entity.LastChangeDateTimeUtc > DateTime.MinValue);
            await _storageService.ResetDeltaStartAsync(cancellationToken);

            foreach (var item in items)
            {
                if (item.IsDeleted == 1)
                {
                    _appDbContext.Set<T>().Remove(item);
                }
                else
                {
                    item.Touch();
                    _appDbContext.Set<T>().Update(item);
                }
            }

            await _appDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> HasOneAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Set<T>().AnyAsync(entity => entity.Id == id, cancellationToken);
        }

        public async Task<bool> HasManyAsync(params Guid?[] ids)
        {
            var idsWithValue = ids.Where(id => id.HasValue).Select(id => id.Value).ToArray();
            if (idsWithValue.Length == 0) return true;

            return await _appDbContext.Set<T>().CountAsync(entity => idsWithValue.Contains(entity.Id)) > 0;
        }

        public async Task<bool> HasManyAsync(IEnumerable<Guid?> ids, CancellationToken cancellationToken = default)
        {
            var idsWithValue = ids
                .Where(id => id.HasValue)
                .Select(id => id.Value)
                .ToArray();
            if (idsWithValue.Length == 0) return true;

            return await _appDbContext.Set<T>()
                .CountAsync(entity => idsWithValue.Contains(entity.Id), cancellationToken) > 0;
        }
    }
}