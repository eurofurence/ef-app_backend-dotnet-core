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
                .Map(dest => dest.ImageIds, src => src.Images.Select(ke => ke.Id));

            // For some reason, the following mapping has to be defined manually
            config
                .NewConfig<KnowledgeEntryRecord, KnowledgeEntryRequest>()
                .Map(dest => dest.Title, src => src.Title)
                .Map(dest => dest.Text, src => src.Text)
                .Map(dest => dest.Order, src => src.Order)
                .Map(dest => dest.Links, src => src.Links.Select(link => new LinkFragment
                {
                    Id = link.Id,
                    Name = link.Name,
                    Target = link.Target
                }).ToList())
                .Map(dest => dest
                    .ImageIds.Select(id => new ImageRecord { Id = id }).ToList(), src => src.Images);
        }
    }
}
