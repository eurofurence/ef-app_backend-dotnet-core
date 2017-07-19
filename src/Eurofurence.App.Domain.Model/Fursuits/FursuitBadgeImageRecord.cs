using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.Fursuits
{
    [DataContract]
    public class FursuitBadgeImageRecord : EntityBase
    {
        [Required]
        [DataMember]
        public string SourceContentHashSha1 { get; set; }

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
        public byte[] ImageBytes { get; set; }
    }
}