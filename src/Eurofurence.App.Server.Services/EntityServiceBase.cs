using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Eurofurence.App.Common.DataDiffUtils;
using Eurofurence.App.Domain.Model;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Sync;
using Eurofurence.App.Server.Services.Abstractions;

namespace Eurofurence.App.Server.Services
{
    public class EntityServiceBase<T> :
        IEntityServiceOperations<T>,
        IPatchOperationProcessor<T>
        where T : EntityBase
    {
        private readonly IEntityRepository<T> _entityRepository;
        private readonly IStorageService _storageService;
        private readonly bool _useSoftDelete;

        public EntityServiceBase(
            IEntityRepository<T> entityRepository,
            IStorageServiceFactory storageServiceFactory,
            bool useSoftDelete = true)
        {
            _entityRepository = entityRepository;
            _storageService = storageServiceFactory.CreateStorageService<T>();
            _useSoftDelete = useSoftDelete;
        }

        public virtual Task<T> FindOneAsync(Guid id)
        {
            return _entityRepository.FindOneAsync(id);
        }

        public virtual Task<IEnumerable<T>> FindAllAsync()
        {
            return _entityRepository.FindAllAsync();
        }

        public Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> filter)
        {
            return _entityRepository.FindAllAsync(filter);
        }

        public virtual async Task ReplaceOneAsync(T entity)
        {
            entity.Touch();
            await _entityRepository.ReplaceOneAsync(entity);
            await _storageService.TouchAsync();
        }

        public virtual async Task InsertOneAsync(T entity)
        {
            entity.Touch();
            await _entityRepository.InsertOneAsync(entity);
            await _storageService.TouchAsync();
        }

        public virtual async Task DeleteOneAsync(Guid id)
        {
            if (_useSoftDelete)
            {
                var entity = await _entityRepository.FindOneAsync(id);
                entity.IsDeleted = 1;
                entity.Touch();

                await _entityRepository.ReplaceOneAsync(entity);
            }
            else
            {
                await _entityRepository.DeleteOneAsync(id);
            }
            await _storageService.TouchAsync();
        }

        public virtual async Task DeleteAllAsync()
        {
            await _entityRepository.DeleteAllAsync();
            await _storageService.ResetDeltaStartAsync();
        }

        public virtual Task<EntityStorageInfoRecord> GetStorageInfoAsync()
        {
            return _storageService.GetStorageInfoAsync();
        }

        public virtual async Task<DeltaResponse<T>> GetDeltaResponseAsync(DateTime? minLastDateTimeChangedUtc = null)
        {
            var storageInfo = await GetStorageInfoAsync();
            var response = new DeltaResponse<T>
            {
                StorageDeltaStartChangeDateTimeUtc = storageInfo.DeltaStartDateTimeUtc,
                StorageLastChangeDateTimeUtc = storageInfo.LastChangeDateTimeUtc
            };

            if (!minLastDateTimeChangedUtc.HasValue || minLastDateTimeChangedUtc < storageInfo.DeltaStartDateTimeUtc)
            {
                response.RemoveAllBeforeInsert = true;
                response.DeletedEntities = new Guid[0];
                response.ChangedEntities =
                    (await _entityRepository.FindAllAsync(false)).ToArray();
            }
            else
            {
                response.RemoveAllBeforeInsert = false;

                var entities = (await _entityRepository.FindAllAsync(true,
                    minLastDateTimeChangedUtc)).ToList();

                response.ChangedEntities = entities.Where(a => a.IsDeleted == 0).ToArray();
                response.DeletedEntities = entities.Where(a => a.IsDeleted == 1).Select(a => a.Id).ToArray();
            }

            return response;
        }

        public virtual async Task ApplyPatchOperationAsync(IEnumerable<PatchOperation<T>> patchResults)
        {
            foreach (var item in patchResults)
                switch (item.Action)
                {
                    case ActionEnum.Add:
                        await InsertOneAsync(item.Entity);
                        break;
                    case ActionEnum.Update:
                        await ReplaceOneAsync(item.Entity);
                        break;
                    case ActionEnum.Delete:
                        await DeleteOneAsync(item.Entity.Id);
                        break;
                }
        }
    }
}