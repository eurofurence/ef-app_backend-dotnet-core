using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Eurofurence.App.Domain.Model.ArtistsAlley;
using Eurofurence.App.Domain.Model.Fursuits;
using Eurofurence.App.Domain.Model.Maps;

namespace Eurofurence.App.Domain.Model.Images
{
    [DataContract]
    public class ImageRecord : EntityBase
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

        public List<FursuitBadgeRecord> FursuitBadges { get; set; } = new();
        public List<TableRegistrationRecord> TableRegistrations { get; set; } = new();
        public List<MapRecord> Maps { get; set; } = new();
    }
}