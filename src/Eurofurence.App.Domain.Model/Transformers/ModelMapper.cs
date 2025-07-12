using System;
using System.Collections.Generic;
using System.Linq;
using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Domain.Model.ArtistsAlley;
using Eurofurence.App.Domain.Model.Communication;
using Eurofurence.App.Domain.Model.Dealers;
using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Domain.Model.Images;
using Eurofurence.App.Domain.Model.Knowledge;
using Eurofurence.App.Domain.Model.LostAndFound;
using Eurofurence.App.Domain.Model.Maps;
using Eurofurence.App.Server.Web.Controllers.Transformers;
using Riok.Mapperly.Abstractions;

namespace Eurofurence.App.Domain.Model.Transformers
{
    [Mapper]
    public static partial class ModelMapper
    {
        public static partial TTarget Map<TTarget>(object entityBase);

        public static partial void Map(AnnouncementRequest car, AnnouncementRecord dto);

        //public static partial void Merge<TSource, TTarger>(TSource source, TTarger target);

        // Mappings for Announcement
        private static partial AnnouncementRecord Map(AnnouncementRequest source);
        private static partial AnnouncementResponse Map(AnnouncementRecord source);

        // Mappings for PrivateMessage
        private static partial PrivateMessageRecord Map(SendPrivateMessageByIdentityRequest source);
        private static partial PrivateMessageRecord Map(SendPrivateMessageByRegSysRequest source);
        private static partial PrivateMessageResponse Map(PrivateMessageRecord source);

        // Mappings for Dealer
        private static partial DealerRecord Map(DealerRequest source);
        private static partial DealerResponse Map(DealerRecord source);


        private static partial EventRecord Map(EventRequest source);
        private static partial EventResponse Map(EventRecord source);

        private static partial ImageRecord Map(ImageRequest source);
        private static partial ImageResponse MapImage(ImageRecord source);

        [MapProperty(nameof(ImageRecord.Announcements), nameof(ImageWithRelationsResponse.AnnouncementIds), Use = nameof(MapEntityIds))]
        [MapProperty(nameof(ImageRecord.KnowledgeEntries), nameof(ImageWithRelationsResponse.KnowledgeEntryIds), Use = nameof(MapEntityIds))]
        [MapProperty(nameof(ImageRecord.TableRegistrations), nameof(ImageWithRelationsResponse.TableRegistrationIds), Use = nameof(MapEntityIds))]
        [MapProperty(nameof(ImageRecord.Maps), nameof(ImageWithRelationsResponse.MapIds), Use = nameof(MapEntityIds))]
        [MapProperty(nameof(ImageRecord.EventBanners), nameof(ImageWithRelationsResponse.EventBannerIds), Use = nameof(MapEntityIds))]
        [MapProperty(nameof(ImageRecord.EventPosters), nameof(ImageWithRelationsResponse.EventPosterIds), Use = nameof(MapEntityIds))]
        [MapProperty(nameof(ImageRecord.DealerArtists), nameof(ImageWithRelationsResponse.DealerArtistIds), Use = nameof(MapEntityIds))]
        [MapProperty(nameof(ImageRecord.DealerArtistThumbnails), nameof(ImageWithRelationsResponse.DealerArtistThumbnailIds), Use = nameof(MapEntityIds))]
        [MapProperty(nameof(ImageRecord.DealerArtPreviews), nameof(ImageWithRelationsResponse.DealerArtPreviewIds), Use = nameof(MapEntityIds))]
        private static partial ImageWithRelationsResponse Map(ImageRecord source);

        private static IList<Guid> MapEntityIds(IList<EntityBase> source)
            => source.Select(x => x.Id).ToList();
        private static partial KnowledgeEntryRecord Map(KnowledgeEntryRequest source);
        private static partial KnowledgeEntryResponse Map(KnowledgeEntryRecord source);

        private static partial KnowledgeGroupRecord Map(KnowledgeGroupRequest source);
        private static partial KnowledgeGroupResponse Map(KnowledgeGroupRecord source);


        private static partial LostAndFoundRecord Map(LostAndFoundRequest source);
        private static partial LostAndFoundResponse Map(LostAndFoundRecord source);


        private static partial MapRecord Map(MapRequest source);
        private static partial MapResponse Map(MapRecord source);


        private static partial TableRegistrationRecord Map(TableRegistrationRequest source);

        private static partial TableRegistrationResponse Map(TableRegistrationRecord source);

        public static partial void Merge<TRequest, TRecord>(IDtoTransformable<TRecord> request, EntityBase target)
            where TRequest : class
            where TRecord : class;
        public static partial void Merge(AnnouncementRequest request, AnnouncementRecord target);

    }
}