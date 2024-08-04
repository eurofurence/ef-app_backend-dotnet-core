using System.Linq;
using Eurofurence.App.Domain.Model.Images;
using Eurofurence.App.Domain.Model.Knowledge;
using Mapster;

namespace Eurofurence.App.Server.Web.Mapper
{
    /// <summary>
    /// Mapping register for mapping IDs of knowledge entry images to the response type
    /// </summary>
    public class KnowledgeEntryResponseRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config
                .NewConfig<KnowledgeEntryRecord, KnowledgeEntryResponse>()
                .Map(dest => dest.ImageIds, src => src.Images.Select(ke => ke.Id));
        }
    }
}
