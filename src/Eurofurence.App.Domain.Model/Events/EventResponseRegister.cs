using Mapster;

namespace Eurofurence.App.Domain.Model.Events
{
    public class EventResponseRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config
                .NewConfig<EventRecord, EventResponse>()
                .Map(dest => dest.FavoredByCount, src => src.FavoredBy.Count);
        }
    }
}
