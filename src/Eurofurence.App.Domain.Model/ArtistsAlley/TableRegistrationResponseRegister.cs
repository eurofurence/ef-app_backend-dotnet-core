using Eurofurence.App.Domain.Model.Images;
using Eurofurence.App.Domain.Model.Transformers;
using Mapster;

namespace Eurofurence.App.Domain.Model.ArtistsAlley
{

    /// <summary>
    /// Configures type adapter mappings for converting TableRegistrationRecord to TableRegistrationResponse.
    /// </summary>
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