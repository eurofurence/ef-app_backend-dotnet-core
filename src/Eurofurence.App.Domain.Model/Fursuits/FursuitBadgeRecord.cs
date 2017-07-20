using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.Fursuits
{
    [DataContract]
    public class FursuitBadgeRecord : EntityBase
    {
        [IgnoreDataMember]
        public string ExternalReference { get; set; }

        [DataMember]
        [Required]
        public string OwnerUid { get; set;  }

        [DataMember]
        [Required]
        public string Name { get; set; }

        [DataMember]
        [Required]
        public string Species { get; set; }

        [DataMember]
        [Required]
        public string Gender { get; set; }

        [DataMember]
        public bool IsPublic { get; set; }
    }
}
