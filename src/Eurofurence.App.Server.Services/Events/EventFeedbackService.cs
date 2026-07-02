using System.Linq;
using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Events;
using Microsoft.EntityFrameworkCore;

namespace Eurofurence.App.Server.Services.Events
{
    public class EventFeedbackService : EntityServiceBase<EventFeedbackRecord, EventFeedbackResponse>,
        IEventFeedbackService
    {
        public EventFeedbackService(
            AppDbContext appDbContext,
            IStorageServiceFactory storageServiceFactory
        )
            : base(appDbContext, storageServiceFactory)
        {
        }

        public override IQueryable<EventFeedbackRecord> FindAll()
        {
            return base.FindAll().Include(x => x.Event);
        }
    }
}