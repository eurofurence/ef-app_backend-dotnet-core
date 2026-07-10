using Eurofurence.App.Domain.Model.Users;
using Mapster;
using System.Linq;

namespace Eurofurence.App.Domain.Model.Events
{
    public class EventStatisticsResponseRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config
                .NewConfig<EventRecord, EventStatisticsResponse>()
                .Map(dest => dest.FavoredByCount, src => src.FavoredBy.Count)
                .Map(dest => dest.FavoredByCheckedInCount, src => src.FavoredBy
                    .GroupBy(x => x.IdentityId).Select(y => y.FirstOrDefault())
                    .Count(e => e.RegistrationStatus == UserRegistrationStatus.CheckedIn));
        }
    }
}
