using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.LostAndFound;

[DataContract]
public class LostAndFoundResponse : ResponseBase
{
    [Required]
    [DataMember]
    public int ExternalId { get; set; }

    [DataMember]
    public string ImageUrl { get; set; }

    [DataMember]
    public string Title { get; set; }

    [DataMember]
    public string Description { get; set; }

    [DataMember]
    public LostAndFoundRecord.LostAndFoundStatusEnum Status { get; set; }

    [DataMember]
    public DateTime? LostDateTimeUtc { get; set; }

    [DataMember]
    public DateTime? FoundDateTimeUtc { get; set; }

    [DataMember]
    public DateTime? ReturnDateTimeUtc { get; set; }
}
