using Eurofurence.App.Domain.Model.Images;
using System;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.Fursuits
{
    [DataContract]
    public class FursuitBadgeResponse : EntityBase
    {
        [DataMember]
        public string ExternalReference { get; set; }

        [DataMember]
        public string OwnerUid { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string WornBy { get; set; }

        [DataMember]
        public string Species { get; set; }

        [DataMember]
        public string Gender { get; set; }

        [DataMember]
        public bool IsPublic { get; set; }

        public string CollectionCode { get; set; }

        public Guid? ImageId { get; set; }
        public ImageResponse Image { get; set; }
    }
}
