using Eurofurence.App.Domain.Model.Maps;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Storage;
using System.Threading.Tasks;
using System;
using System.Threading;

namespace Eurofurence.App.Server.Services.Abstractions.Maps
{
    public interface IMapService :
        IEntityServiceOperations<MapRecord>,
        IPatchOperationProcessor<MapRecord>
    {
        public Task InsertOneEntryAsync(MapEntryRecord entity, CancellationToken cancellationToken = default);
        public Task ReplaceOneEntryAsync(MapEntryRecord entity, CancellationToken cancellationToken = default);
        public Task DeleteOneEntryAsync(Guid id, CancellationToken cancellationToken = default);
        public Task DeleteAllEntriesAsync(Guid id, CancellationToken cancellationToken = default);
    }
}