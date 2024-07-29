using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.Images
{
    [DataContract]
    public class ImageResponse : EntityBase
    {
        [DataMember]
        public string InternalReference { get; set; }

        [DataMember]
        public int Width { get; set; }

        [DataMember]
        public int Height { get; set; }

        [DataMember]
        public long SizeInBytes { get; set; }

        [DataMember]
        public string MimeType { get; set; }

        [DataMember]
        public string ContentHashSha1 { get; set; }
    }
}
