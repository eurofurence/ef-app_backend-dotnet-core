using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Eurofurence.App.Domain.Model.Transformers;

namespace Eurofurence.App.Domain.Model.Maps;

[DataContract]
public class MapRequest : IDtoTransformable<MapRecord>
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
    public IList<MapEntryRequest> Entries { get; set; } = new List<MapEntryRequest>();

    [DataMember]
    public Guid? ImageId { get; set; }
}
