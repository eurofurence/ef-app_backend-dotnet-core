using Mapster;

namespace Eurofurence.App.Domain.Model.Events;

public class EventFeedbackResponseRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<EventFeedbackRecord, EventFeedbackResponse>()
            .Map(dest => dest.EventSlug, src => src.Event.Slug)
            .Map(dest => dest.EventSourceId, src => src.Event.SourceId)
            .PreserveReference(true);
    }
}