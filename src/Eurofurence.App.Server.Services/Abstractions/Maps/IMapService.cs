using Eurofurence.App.Domain.Model.Maps;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Storage;
using System.Threading.Tasks;
using System;

namespace Eurofurence.App.Server.Services.Abstractions.Maps
{
    public interface IMapService :
        IEntityServiceOperations<MapRecord>,
        IPatchOperationProcessor<MapRecord>
    {
        public Task InsertOneEntryAsync(MapEntryRecord entity);
        public Task ReplaceOneEntryAsync(MapEntryRecord entity);
        public Task DeleteOneEntryAsync(Guid id);
        public Task DeleteAllEntriesAsync(Guid id);
    }
}