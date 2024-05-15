using System;
using System.Linq;
using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Events;

namespace Eurofurence.App.Server.Services.Events
{
    public class EventService : EntityServiceBase<EventRecord>,
        IEventService
    {
        public EventService(
            AppDbContext appDbContext,
            IStorageServiceFactory storageServiceFactory
        )
            : base(appDbContext, storageServiceFactory)
        {
        }

        public IQueryable<EventRecord> FindConflicts(DateTime conflictStartTime, DateTime conflictEndTime, TimeSpan tolerance)
        {
            var queryConflictEndTime = conflictEndTime + tolerance;
            var queryConflictStartTime = conflictStartTime - tolerance;

            return FindAll().Where(e => 
                e.IsDeleted == 0 &&
                e.StartDateTimeUtc <= queryConflictEndTime &&
                e.EndDateTimeUtc >= queryConflictStartTime);
        }
    }
}