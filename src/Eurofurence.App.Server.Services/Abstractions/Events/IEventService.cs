using Eurofurence.App.Domain.Model.Events;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions.Events
{
    public interface IEventService :
        IEntityServiceOperations<EventRecord>,
        IPatchOperationProcessor<EventRecord>
    {
        IQueryable<EventRecord> FindConflicts(
            DateTime conflictStartTime,
            DateTime conflictEndTime,
            TimeSpan tolerance);

        public Task RunImportAsync();
    }
}