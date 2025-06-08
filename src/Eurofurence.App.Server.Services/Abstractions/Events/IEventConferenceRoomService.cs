using Eurofurence.App.Domain.Model.Events;

namespace Eurofurence.App.Server.Services.Abstractions.Events
{
    public interface IEventConferenceRoomService :
        IEntityServiceOperations<EventConferenceRoomRecord, EventConferenceRoomResponse>,
        IPatchOperationProcessor<EventConferenceRoomRecord>
    { }
}