using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Server.Services.Abstractions;

namespace Eurofurence.App.Server.Services.Storage
{
    public class StorageServiceFactory : IStorageServiceFactory
    {
        private readonly IEntityStorageInfoRepository _entityStorageInfoRepository;

        public StorageServiceFactory(IEntityStorageInfoRepository entityStorageInfoRepository)
        {
            _entityStorageInfoRepository = entityStorageInfoRepository;
        }

        public StorageService<T> CreateStorageService<T>()
        {
            return new StorageService<T>(_entityStorageInfoRepository, typeof(T).Name);
        }
    }
}