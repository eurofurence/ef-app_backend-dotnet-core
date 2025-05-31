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

    protected bool Equals(ImageResponse other) => Id.Equals(other.Id) && InternalReference == other.InternalReference && Width == other.Width && Height == other.Height && SizeInBytes == other.SizeInBytes && MimeType == other.MimeType && ContentHashSha1 == other.ContentHashSha1 && Url == other.Url && IsRestricted == other.IsRestricted;
    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ImageResponse)obj);
    }
    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Id);
        hashCode.Add(InternalReference);
        hashCode.Add(Width);
        hashCode.Add(Height);
        hashCode.Add(SizeInBytes);
        hashCode.Add(MimeType);
        hashCode.Add(ContentHashSha1);
        hashCode.Add(Url);
        hashCode.Add(IsRestricted);
        return hashCode.ToHashCode();
    }

}