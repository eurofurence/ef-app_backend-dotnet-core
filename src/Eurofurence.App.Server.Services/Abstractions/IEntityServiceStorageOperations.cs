using System;
using System.Threading;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model;
using Eurofurence.App.Domain.Model.Sync;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public interface IEntityServiceStorageOperations<TEntity> where TEntity : EntityBase
    {
        Task<DeltaResponse<TEntity>> GetDeltaResponseAsync(
            DateTime? minLastDateTimeChangedUtc = null,
            CancellationToken cancellationToken = default);

        Task<EntityStorageInfoRecord> GetStorageInfoAsync(CancellationToken cancellationToken = default);
        Task ResetStorageDeltaAsync(CancellationToken cancellationToken = default);
    }
}