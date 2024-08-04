using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Eurofurence.App.Domain.Model.Images;

namespace Eurofurence.App.Domain.Model.Fursuits
{
    [DataContract]
    public class FursuitBadgeRecord : EntityBase
    {
        [DataMember]
        public string ExternalReference { get; set; }

        [DataMember]
        [Required]
        public string OwnerUid { get; set;  }

        [DataMember]
        [Required]
        public string Name { get; set; }

        [DataMember]
        [Required]
        public string WornBy { get; set; }

        [DataMember]
        [Required]
        public string Species { get; set; }

        [DataMember]
        [Required]
        public string Gender { get; set; }

        [DataMember]
        public bool IsPublic { get; set; }

        [DataMember]
        public string CollectionCode { get; set; }

        [DataMember]
        public Guid? ImageId { get; set; }

        [JsonIgnore]
        public ImageRecord Image { get; set; }
    }
}
