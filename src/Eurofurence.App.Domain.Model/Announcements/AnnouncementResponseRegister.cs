using Mapster;

namespace Eurofurence.App.Domain.Model.Announcements
{
    public class AnnouncementResponseRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<AnnouncementRecord, AnnouncementResponse>()
                .Map(x => x.Roles, x => x.Groups);
        }
    }
}