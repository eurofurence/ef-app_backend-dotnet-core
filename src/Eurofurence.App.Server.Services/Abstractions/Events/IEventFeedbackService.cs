using Eurofurence.App.Domain.Model.Events;

namespace Eurofurence.App.Server.Services.Abstractions.Events
{
    public interface IEventFeedbackService :
        IEntityServiceOperations<EventFeedbackRecord, EventFeedbackResponse>,
        IPatchOperationProcessor<EventFeedbackRecord>
    {
    }
}