using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.Announcements
{
    [DataContract]
    public class IdentityAnnouncementGroupsRecord : EntityBase
    {
        [Required]
        [DataMember]
        public string IdentityId { get; set; }

        [Required]
        [DataMember]
        public string[] Groups { get; set; }
    }
}
