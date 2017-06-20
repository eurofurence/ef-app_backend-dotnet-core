using Eurofurence.App.Domain.Model.Events;

namespace Eurofurence.App.Server.Services.Abstractions.Events
{
    public interface IEventService :
        IEntityServiceOperations<EventRecord>,
        IPatchOperationProcessor<EventRecord>
    {
    }
}