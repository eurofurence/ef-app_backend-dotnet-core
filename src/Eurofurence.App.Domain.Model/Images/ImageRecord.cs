using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

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
        public string MimeType{ get; set; }

        [Required]
        [DataMember]
        public string ContentHashSha1 { get; set; }
    }
}
