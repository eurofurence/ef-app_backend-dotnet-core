using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions;

namespace Eurofurence.App.Server.Services.Storage
{
    public class StorageServiceFactory : IStorageServiceFactory
    {
        private readonly AppDbContext _appDbContext;

        public StorageServiceFactory(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public StorageService<T> CreateStorageService<T>()
        {
            return new StorageService<T>(_appDbContext, typeof(T).Name);
        }
    }
}