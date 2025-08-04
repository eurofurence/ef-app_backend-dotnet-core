using Eurofurence.App.Domain.Model.Images;
using Eurofurence.App.Domain.Model.Transformers;
using Mapster;

namespace Eurofurence.App.Domain.Model.ArtistsAlley
{

    /// <summary>
    /// Configures type adapter mappings for converting TableRegistrationRecord to ArtistAlleyResponse.
    /// </summary>
    public class ArtistAlleyResponseRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<TableRegistrationRecord, ArtistAlleyResponse>();
        }
    }
}