using Eurofurence.App.Server.Services.Storage;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public interface IStorageServiceFactory
    {
        StorageService<T> CreateStorageService<T>();
    }
}