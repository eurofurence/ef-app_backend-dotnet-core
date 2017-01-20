using Eurofurence.App.Domain.Model;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public interface IEventService :
        IEntityServiceOperations<EventRecord>,
        IPatchOperationProcessor<EventRecord>
    {

    }
}