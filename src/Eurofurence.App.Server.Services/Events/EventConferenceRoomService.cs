using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Events;

namespace Eurofurence.App.Server.Services.Events
{
    public class EventConferenceRoomService : EntityServiceBase<EventConferenceRoomRecord>,
        IEventConferenceRoomService
    {
        public EventConferenceRoomService(
            AppDbContext appDbContext,
            IStorageServiceFactory storageServiceFactory
        )
            : base(appDbContext, storageServiceFactory)
        {
        }
    }
}