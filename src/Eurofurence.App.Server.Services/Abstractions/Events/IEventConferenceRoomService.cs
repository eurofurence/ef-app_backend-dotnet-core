using Eurofurence.App.Domain.Model.Events;
using System;

namespace Eurofurence.App.Server.Services.Abstractions.Events
{
    public interface IEventConferenceRoomService :
        IEntityServiceOperations<EventConferenceRoomRecord, EventConferenceRoomResponse>,
        IPatchOperationProcessor<EventConferenceRoomRecord>
    {
        public string GetMapLink(Guid id);
    }
}