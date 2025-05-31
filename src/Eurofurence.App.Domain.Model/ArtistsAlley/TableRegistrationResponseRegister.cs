using Eurofurence.App.Domain.Model.Images;
using Eurofurence.App.Server.Web.Controllers.Transformers;
using Mapster;

namespace Eurofurence.App.Domain.Model.ArtistsAlley
{
    public class TableRegistrationResponseRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config
                .NewConfig<TableRegistrationRecord, TableRegistrationResponse>()
                .Map(dest => dest.Image, src => src.Image.Transform<ImageResponse>())
                .PreserveReference(true);
        }
    }
}