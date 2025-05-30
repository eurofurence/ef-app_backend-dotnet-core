using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.Images;

public class ImageResponse : ResponseBase
{
    [DataMember]
    public Guid Id { get; set; }

    [Required]
    [DataMember]
    public string InternalReference { get; set; }

    [Required]
    [DataMember]
    public int Width { get; set; }

    [Required]
    [DataMember]
    public int Height { get; set; }

    [Required]
    [DataMember]
    public long SizeInBytes { get; set; }

    [Required]
    [DataMember]
    public string MimeType { get; set; }

    [Required]
    [DataMember]
    public string ContentHashSha1 { get; set; }

    [Required]
    [DataMember]
    public string Url { get; set; }

    [Required]
    [DataMember]
    public bool IsRestricted { get; set; }

}