using Eurofurence.App.Domain.Model;
using Eurofurence.App.Domain.Model.Events;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public interface IEventConferenceRoomService :
        IEntityServiceOperations<EventConferenceRoomRecord>,
        IPatchOperationProcessor<EventConferenceRoomRecord>
    {

    }
}