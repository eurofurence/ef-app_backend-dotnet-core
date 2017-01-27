using Eurofurence.App.Domain.Model;
using Eurofurence.App.Domain.Model.Events;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public interface IEventConferenceDayService :
        IEntityServiceOperations<EventConferenceDayRecord>,
        IPatchOperationProcessor<EventConferenceDayRecord>
    {

    }
}