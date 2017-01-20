using Eurofurence.App.Domain.Model;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public interface IEventConferenceTrackService : 
        IEntityServiceOperations<EventConferenceTrackRecord>,
        IPatchOperationProcessor<EventConferenceTrackRecord>
    {
        
    }
}