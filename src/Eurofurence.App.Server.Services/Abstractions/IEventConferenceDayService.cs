using Eurofurence.App.Domain.Model;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public interface IEventConferenceDayService :
        IEntityServiceOperations<EventConferenceDayRecord>,
        IPatchOperationProcessor<EventConferenceDayRecord>
    {

    }
}