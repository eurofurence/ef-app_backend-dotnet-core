using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Domain.Model.ArtistsAlley;
using Eurofurence.App.Domain.Model.Dealers;
using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Domain.Model.Knowledge;
using Eurofurence.App.Domain.Model.Maps;
using Eurofurence.App.Domain.Model.Transformers;

namespace Eurofurence.App.Domain.Model.Images
{
    [DataContract]
    public class ImageRecord : EntityBase, IDtoRecordTransformable<ImageRequest, ImageResponse, ImageRecord>
    {
        [Required]
        [DataMember]
        public string InternalReference { get; set; }

        [Required]
        [DataMember]
        public int Width { get; set; }

        [Required]
        [DataMember]
        public int Height { get; set; }

        [Required]
        [DataMember]
        public long SizeInBytes { get; set; }

        [Required]
        [DataMember]
        public string MimeType { get; set; }

        [Required]
        [DataMember]
        public string ContentHashSha1 { get; set; }

        [Required]
        [DataMember]
        public string Url { get; set; }

        [Required]
        [DataMember]
        public bool IsRestricted { get; set; }

        [Required]
        [JsonIgnore]
        public string InternalFileName { get; set; }

        [JsonIgnore]
        public virtual List<AnnouncementRecord> Announcements { get; set; } = new();

        [JsonIgnore]
        public virtual List<KnowledgeEntryRecord> KnowledgeEntries { get; set; } = new();

        [JsonIgnore]
        public virtual List<TableRegistrationRecord> TableRegistrations { get; set; } = new();

        [JsonIgnore]
        public virtual List<MapRecord> Maps { get; set; } = new();

        [JsonIgnore]
        public virtual List<EventRecord> EventBanners { get; set; } = new();

        [JsonIgnore]
        public virtual List<EventRecord> EventPosters { get; set; } = new();

        [JsonIgnore]
        public virtual List<DealerRecord> DealerArtists { get; set; } = new();

        [JsonIgnore]
        public virtual List<DealerRecord> DealerArtistThumbnails { get; set; } = new();

        [JsonIgnore]
        public virtual List<DealerRecord> DealerArtPreviews { get; set; } = new();
    }
}
