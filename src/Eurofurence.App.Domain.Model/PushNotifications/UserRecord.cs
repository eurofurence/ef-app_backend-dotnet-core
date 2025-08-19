using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Domain.Model.Users;
using Eurofurence.App.Domain.Model.Transformers;

namespace Eurofurence.App.Domain.Model.PushNotifications;

public class UserRecord : EntityBase, IDtoTransformable<UserResponse>
{
    [Required]
    [DataMember]
    public string RegSysId { get; set; }

    [Required]
    [DataMember]
    public string IdentityId { get; set; }

    [DataMember]
    public string Nickname { get; set; }

    public List<EventRecord> FavoriteEvents { get; set; } = new();

#nullable enable
    public string? CalendarToken { get; set; }
}