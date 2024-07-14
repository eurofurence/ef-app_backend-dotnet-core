using Eurofurence.App.Domain.Model.Images;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.Maps
{
    [DataContract]
    public class MapRecord : EntityBase
    {
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
        public IList<MapEntryRecord> Entries { get; set; } = new List<MapEntryRecord>();

        [DataMember]
        [Required]
        public Guid ImageId { get; set; }

        [IgnoreDataMember]
        public virtual ImageRecord Image { get; set; }

    }
}