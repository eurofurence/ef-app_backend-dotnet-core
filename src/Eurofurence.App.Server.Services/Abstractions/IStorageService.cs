using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Sync;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public interface IStorageService
    {
        Task TouchAsync();
        Task ResetDeltaStartAsync();
        Task<EntityStorageInfoRecord> GetStorageInfoAsync();
    }
}