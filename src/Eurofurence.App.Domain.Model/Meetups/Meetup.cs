using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Eurofurence.App.Domain.Model.Meetups
{
    [DataContract]
    public class Meetup : EntityBase
    {
        [DataMember]
        [Required]
        public string Title { get; set; }

        [DataMember]
        [Required]
        public string HostUid { get; set; }

        [DataMember]
        [Required]
        public DateTime CreatedDateTimeUtc { get; set; }

        [DataMember]
        public DateTime StartDateTimeUtc { get; set; }

        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public string Location { get; set; }
        [DataMember]
        public int ExpectedDurationMinutes { get; set; }

    }
}
