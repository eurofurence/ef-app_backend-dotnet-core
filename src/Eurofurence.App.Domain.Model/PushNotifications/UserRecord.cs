#nullable enable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Domain.Model.Transformers;
using Eurofurence.App.Domain.Model.Users;

namespace Eurofurence.App.Domain.Model.PushNotifications;

public class UserRecord : EntityBase, IDtoTransformable<UserResponse>
{
    /// <summary>
    /// ID of the user in the registration system if they have a valid registration.
    /// </summary>
    [DataMember]
    public string? RegSysId { get; set; }

    /// <summary>
    /// Identity provider ID of the user.
    /// </summary>
    [Required]
    [DataMember]
    public required string IdentityId { get; set; }

    [DataMember]
    public string? Nickname { get; set; }

    /// <summary>
    /// Status of the user's registration based on <c>RegSysId</c>.
    /// Will be <c>UserRegistrationStatus.Unknown</c> if no registration was found or status could
    /// not be retrieved successfully.
    /// </summary>
    [Required]
    [DataMember]
    public UserRegistrationStatus RegistrationStatus { get; set; } = UserRegistrationStatus.Unknown;

    /// <summary>
    /// List of events the user has added to their favorites; used for calendar sync and statistics.
    /// </summary>
    public List<EventRecord> FavoriteEvents { get; set; } = new();

    /// <summary>
    /// Persistent token that can be used by the users calendar app to fetch their favorite events
    /// in iCal format.
    /// </summary>
    public string? CalendarToken { get; set; }
}