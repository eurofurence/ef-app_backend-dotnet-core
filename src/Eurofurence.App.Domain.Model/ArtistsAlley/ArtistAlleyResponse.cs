using System;
using System.Runtime.Serialization;
using Eurofurence.App.Domain.Model.Images;

namespace Eurofurence.App.Domain.Model.ArtistsAlley;

/// <summary>
/// Data type used to present ARt
/// </summary>
[DataContract]
public class ArtistAlleyResponse : ResponseBase
{
    /// <summary>
    /// Date and time at which the registration was submitted.
    /// </summary>
    [DataMember]
    public DateTime CreatedDateTimeUtc { get; set; }

    /// <summary>
    /// Preferred display name of artist.
    /// </summary>
    [DataMember]
    public string DisplayName { get; set; }

    /// <summary>
    /// Optional URL of artist's website.
    /// </summary>
    [DataMember]
    public string WebsiteUrl { get; set; }

    /// <summary>
    /// Short text provided by the artist on who they are and what they are offering.
    /// </summary>
    [DataMember]
    public string ShortDescription { get; set; }

    /// <summary>
    /// Optional Telegram handle (prefixed @ will be removed automatically).
    /// </summary>
    [DataMember]
    public string TelegramHandle { get; set; }

    /// <summary>
    /// Table number at which the artist has seated themselves in the Artist Alley.
    /// Must be > 0 and may have optional, upper limit. 
    /// </summary>
    [DataMember]
    public string Location { get; set; }

    /// <summary>
    /// ID of the image attached to this registration.
    /// </summary>
    [DataMember]
    public Guid? ImageId { get; set; }
}