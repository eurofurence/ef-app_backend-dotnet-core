using Eurofurence.App.Domain.Model.Events;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions.Events
{
    public interface IEventService :
        IEntityServiceOperations<EventRecord>,
        IPatchOperationProcessor<EventRecord>
    {
        Task<IEnumerable<EventRecord>> FindConflictsAsync(
            DateTime conflictStartTime,
            DateTime conflictEndTime,
            TimeSpan tolerance);
    }
}