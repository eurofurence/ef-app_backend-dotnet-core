using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Eurofurence.App.Domain.Model.Images;

namespace Eurofurence.App.Domain.Model.ArtistsAlley;

[DataContract]
public class TableRegistrationResponse : ResponseBase
{
    /// <summary>
    /// Date and time at which the registration was submitted.
    /// </summary>
    [DataMember]
    public DateTime CreatedDateTimeUtc { get; set; }

    /// <summary>
    /// Identity provider ID of the user that submitted the registration.
    /// </summary>
    [DataMember]
    public string OwnerUid { get; set; }

    /// <summary>
    /// Actual name of the user (may be different from `DisplayName`).
    /// </summary>
    [DataMember]
    public string OwnerUsername { get; set; }

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

    /// <summary>
    /// Metadata of the image attached to this registration.
    /// </summary>
    [DataMember]
    public ImageRecord Image { get; set; }

    /// <summary>
    /// Current state of the registration (Pending, Accepted, Published, Rejected).
    /// </summary>
    [DataMember]
    public TableRegistrationRecord.RegistrationStateEnum State { get; set; } = TableRegistrationRecord.RegistrationStateEnum.Pending;

    /// <summary>
    /// Internal log of state changes this registration has undergone.
    /// </summary>
    [JsonIgnore]
    public IList<ArtistAlleyUserPenaltyRecord.StateChangeRecord> StateChangeLog { get; set; }
}