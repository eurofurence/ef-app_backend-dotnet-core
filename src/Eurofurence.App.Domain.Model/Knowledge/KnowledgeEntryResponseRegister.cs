using System.Linq;
using Eurofurence.App.Domain.Model.Fragments;
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
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.ImageIds, src => src.Images.Select(ke => ke.Id))
                .IgnoreNonMapped(true)
                .PreserveReference(true);

            // For some reason, the following mapping has to be defined manually
            config
                .NewConfig<KnowledgeEntryRequest, KnowledgeEntryRecord>()
                .Map(dest => dest.Images, src => src.ImageIds.Select(x => new ImageRecord() { Id = x }));
        }
    }
}
