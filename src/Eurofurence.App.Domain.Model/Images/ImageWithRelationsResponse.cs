using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.Images
{
    [DataContract]
    public class ImageWithRelationsResponse : ImageRecord
    {
        [DataMember]
        public List<Guid> AnnouncementIds { get; set; } = new();

        [DataMember]
        public List<Guid> KnowledgeEntryIds { get; set; } = new();

        [DataMember]
        public List<Guid> FursuitBadgeIds { get; set; } = new();

        [DataMember]
        public List<Guid> TableRegistrationIds { get; set; } = new();

        [DataMember] 
        public List<Guid> MapIds { get; set; } = new();

        [DataMember]
        public List<Guid> EventBannerIds { get; set; } = new();

        [DataMember]
        public List<Guid> EventPosterIds { get; set; } = new();

        [DataMember]
        public List<Guid> DealerArtistIds { get; set; } = new();

        [DataMember]
        public List<Guid> DealerArtistThumbnailIds { get; set; } = new();

        [DataMember]
        public List<Guid> DealerArtPreviewIds { get; set; } = new();
    }
}
