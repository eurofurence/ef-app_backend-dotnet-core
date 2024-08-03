using System.Linq;
using Mapster;
using Eurofurence.App.Domain.Model.Images;

namespace Eurofurence.App.Server.Web.Mapper
{
    /// <summary>
    /// Mapping register for mapping IDs of image relations to the response type
    /// </summary>
    public class ImageWithRelationsResponseRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config
                .NewConfig<ImageRecord, ImageWithRelationsResponse>()
                .Map(dest => dest.KnowledgeEntryIds, src => src.KnowledgeEntries.Select(ke => ke.Id))
                .Map(dest => dest.FursuitBadgeIds, src => src.FursuitBadges.Select(fb => fb.Id))
                .Map(dest => dest.TableRegistrationIds, src => src.TableRegistrations.Select(tr => tr.Id))
                .Map(dest => dest.MapIds, src => src.Maps.Select(m => m.Id))
                .Map(dest => dest.EventBannerIds, src => src.EventBanners.Select(eb => eb.Id))
                .Map(dest => dest.EventPosterIds, src => src.EventPosters.Select(ep => ep.Id))
                .Map(dest => dest.DealerArtistIds, src => src.DealerArtists.Select(da => da.Id))
                .Map(dest => dest.DealerArtistThumbnailIds, src => src.DealerArtistThumbnails.Select(dat => dat.Id))
                .Map(dest => dest.DealerArtPreviewIds, src => src.DealerArtPreviews.Select(dap => dap.Id))
                .Map(dest => dest.AnnouncementIds, src => src.Announcements.Select(a => a.Id));
        }
    }
}
