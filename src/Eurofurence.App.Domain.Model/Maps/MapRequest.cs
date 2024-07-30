using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.Maps
{
    [DataContract]
    public class MapRequest
    {
        public Guid Id { get; set; }

        [DataMember]
        [Required]
        public string Description { get; set; }

        [Required]
        [DataMember]
        public int Order { get; set; }

        [DataMember]
        [Required]
        public bool IsBrowseable { get; set; }

        [DataMember]
        public List<MapEntryRequest> Entries { get; set; } = new();

        [DataMember]
        [Required]
        public Guid ImageId { get; set; }
    }
}
