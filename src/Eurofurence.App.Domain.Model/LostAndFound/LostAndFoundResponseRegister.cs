using Eurofurence.App.Domain.Model.LostAndFound;
using Mapster;

namespace Eurofurence.App.Server.Web.Mapper
{
    /// <summary>
    /// Mapping register for mapping IDs of lost and found records to the response type
    /// </summary>
    public class LostAndFoundResponseRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config
                .NewConfig<LostAndFoundRecord, LostAndFoundResponse>()
                //FIXME: workaround for EF29 version of app using LastChangeDateTimeUtc instead of Found/LostDateTimeUtc
                .Map(dest => dest.LastChangeDateTimeUtc, src => src.Status == LostAndFoundRecord.LostAndFoundStatusEnum.Found ? src.FoundDateTimeUtc : src.LostDateTimeUtc)
                .PreserveReference(true);
        }
    }
}
