using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Eurofurence.App.Common.DataDiffUtils;
using Eurofurence.App.Domain.Model;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public interface IPatchOperationProcessor<TEntity> where TEntity : EntityBase
    {
        Task ApplyPatchOperationAsync(
            IEnumerable<PatchOperation<TEntity>> patchResults,
            CancellationToken cancellationToken = default);
    }
}