using Eurofurence.App.Domain.Model;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Server.Services.Abstractions;

namespace Eurofurence.App.Server.Services.Events
{
    public class EventConferenceTrackService : EntityServiceBase<EventConferenceTrackRecord>, 
        IEventConferenceTrackService
    {
        public EventConferenceTrackService(IEntityRepository<EventConferenceTrackRecord> entityRepository) 
            : base(entityRepository)
        {
        }
    }
}

