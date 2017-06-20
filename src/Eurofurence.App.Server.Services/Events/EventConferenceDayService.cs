using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Events;

namespace Eurofurence.App.Server.Services.Events
{
    public class EventConferenceDayService : EntityServiceBase<EventConferenceDayRecord>,
        IEventConferenceDayService
    {
        public EventConferenceDayService(
            IEntityRepository<EventConferenceDayRecord> entityRepository,
            IStorageServiceFactory storageServiceFactory
            )
            : base(entityRepository, storageServiceFactory)
        {
        }
    }
}