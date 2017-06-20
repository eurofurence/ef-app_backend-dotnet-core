using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Events;

namespace Eurofurence.App.Server.Services.Events
{
    public class EventFeedbackService : EntityServiceBase<EventFeedbackRecord>,
        IEventFeedbackService
    {
        public EventFeedbackService(
            IEntityRepository<EventFeedbackRecord> entityRepository,
            IStorageServiceFactory storageServiceFactory
        )
            : base(entityRepository, storageServiceFactory)
        {
        }
    }
}