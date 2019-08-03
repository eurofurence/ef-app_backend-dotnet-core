using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Events;

namespace Eurofurence.App.Server.Services.Events
{
    public class EventService : EntityServiceBase<EventRecord>,
        IEventService
    {
        public EventService(
            IEntityRepository<EventRecord> entityRepository,
            IStorageServiceFactory storageServiceFactory
        )
            : base(entityRepository, storageServiceFactory)
        {
        }

        public Task<IEnumerable<EventRecord>> FindConflictsAsync(DateTime conflictStartTime, DateTime conflictEndTime, TimeSpan tolerance)
        {
            var queryConflictEndTime = conflictEndTime + tolerance;
            var queryConflictStartTime = conflictStartTime - tolerance;

            return FindAllAsync(@event => 
                @event.IsDeleted == 0 &&
                @event.StartDateTimeUtc <= queryConflictEndTime &&
                @event.EndDateTimeUtc >= queryConflictStartTime);
        }
    }
}