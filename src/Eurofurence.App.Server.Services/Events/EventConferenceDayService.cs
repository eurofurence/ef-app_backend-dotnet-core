using Eurofurence.App.Domain.Model;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Server.Services.Abstractions;

namespace Eurofurence.App.Server.Services.Events
{
    public class EventConferenceDayService : EntityServiceBase<EventConferenceDayRecord>,
        IEventConferenceDayService
    {
        public EventConferenceDayService(IEntityRepository<EventConferenceDayRecord> entityRepository)
            : base(entityRepository)
        {
        }
    }
}