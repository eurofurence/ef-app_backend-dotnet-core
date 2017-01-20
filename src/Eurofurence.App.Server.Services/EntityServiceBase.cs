using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eurofurence.App.Common.DataDiffUtils;
using Eurofurence.App.Domain.Model;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Server.Services.Abstractions;

namespace Eurofurence.App.Server.Services
{
    public class EntityServiceBase<T> : 
        IEntityServiceOperations<T>,
        IPatchOperationProcessor<T>
        where T : EntityBase
    {
        private readonly IEntityRepository<T> _entityRepository;

        public EntityServiceBase(IEntityRepository<T> entityRepository)
        {
            _entityRepository = entityRepository;
        }
        public Task<T> FindOneAsync(Guid id)
        {
            return _entityRepository.FindOneAsync(id);
        }

        public Task<IEnumerable<T>> FindAllAsync()
        {
            return _entityRepository.FindAllAsync();
        }

        public Task ReplaceOneAsync(T entity)
        {
            return _entityRepository.ReplaceOneAsync(entity);
        }

        public Task InsertOneAsync(T entity)
        {
            return _entityRepository.InsertOneAsync(entity);
        }

        public async Task DeleteOneAsync(Guid id)
        {
            var entity = await _entityRepository.FindOneAsync(id);
            entity.IsDeleted = 1;
            entity.Touch();

            await _entityRepository.ReplaceOneAsync(entity);
        }

        public async Task ApplyPatchOperationAsync(IEnumerable<PatchOperation<T>> patchResults)
        {
            foreach (var item in patchResults)
            {
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
}