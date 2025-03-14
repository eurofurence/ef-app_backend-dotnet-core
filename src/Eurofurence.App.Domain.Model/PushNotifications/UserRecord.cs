using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Eurofurence.App.Domain.Model.Events;

namespace Eurofurence.App.Domain.Model.PushNotifications;

public class UserRecord : EntityBase
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