using Eurofurence.App.Domain.Model;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Events;

namespace Eurofurence.App.Server.Services.Events
{
    public class EventService : EntityServiceBase<EventRecord>,
        IEventService
    {
        public EventService(
            IEntityRepository<EventRecord> entityRepository,
            IStorageServiceFactory storageServiceFactory
            )
            : base(entityRepository, storageServiceFactory)
        {
        }
    }
}