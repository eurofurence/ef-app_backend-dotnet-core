using Eurofurence.App.Domain.Model;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public interface IEventConferenceRoomService :
        IEntityServiceOperations<EventConferenceRoomRecord>,
        IPatchOperationProcessor<EventConferenceRoomRecord>
    {

    }
}