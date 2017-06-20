using Eurofurence.App.Domain.Model.Events;

namespace Eurofurence.App.Server.Services.Abstractions.Events
{
    public interface IEventConferenceTrackService :
        IEntityServiceOperations<EventConferenceTrackRecord>,
        IPatchOperationProcessor<EventConferenceTrackRecord>
    {
    }
}