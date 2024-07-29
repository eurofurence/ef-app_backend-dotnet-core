using Eurofurence.App.Domain.Model.Images;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.Maps
{
    [DataContract]
    public class MapResponse : EntityBase
    {
        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public int Order { get; set; }

        [DataMember]
        public bool IsBrowseable { get; set; }

        [DataMember]
        public List<MapEntryResponse> Entries { get; set; } = new();

        [DataMember]
        public Guid ImageId { get; set; }

        [IgnoreDataMember]
        public virtual ImageResponse Image { get; set; }
    }
}
