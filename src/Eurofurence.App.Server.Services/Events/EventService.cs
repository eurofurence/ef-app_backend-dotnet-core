using Eurofurence.App.Domain.Model;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Server.Services.Abstractions;

namespace Eurofurence.App.Server.Services.Events
{
    public class EventService : EntityServiceBase<EventRecord>,
        IEventService
    {
        public EventService(IEntityRepository<EventRecord> entityRepository)
            : base(entityRepository)
        {
        }
    }
}