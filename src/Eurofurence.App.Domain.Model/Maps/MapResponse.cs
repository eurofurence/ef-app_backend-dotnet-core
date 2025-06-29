#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.Maps;

[DataContract]
public class MapResponse : ResponseBase
{
    [DataMember]
    [Required]
    public new Guid Id { get; set; }

    [DataMember]
    [Required]
    public required string Description { get; set; }

    [DataMember]
    [Required]
    public int Order { get; set; }

    [DataMember]
    [Required]
    public bool IsBrowseable { get; set; }

    [DataMember]
    public IList<MapEntryResponse>? Entries { get; set; } = new List<MapEntryResponse>();

    [DataMember]
    public Guid? ImageId { get; set; }
}
