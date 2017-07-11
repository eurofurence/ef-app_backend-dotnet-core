using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.Announcements
{
    [DataContract]
    public class AnnouncementRecord : EntityBase
    {
        [DataMember]
        [Required]
        public DateTime ValidFromDateTimeUtc { get; set; }

        [DataMember]
        [Required]
        public DateTime ValidUntilDateTimeUtc { get; set; }

        [IgnoreDataMember]
        public string ExternalReference { get; set; }

        [DataMember]
        [Required]
        public string Area { get; set; }

        [DataMember]
        [Required]
        public string Author { get; set; }

        [DataMember]
        [Required]
        public string Title { get; set; }

        [DataMember]
        [Required]
        public string Content { get; set; }
    }
}