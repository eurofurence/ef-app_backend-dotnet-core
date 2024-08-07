using System.Threading;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Sync;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public interface IStorageService
    {
        Task TouchAsync(CancellationToken cancellationToken = default);
        Task ResetDeltaStartAsync(CancellationToken cancellationToken = default);
        Task<EntityStorageInfoRecord> GetStorageInfoAsync(CancellationToken cancellationToken = default);
    }
}