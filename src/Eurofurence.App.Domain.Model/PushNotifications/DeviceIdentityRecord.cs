using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.PushNotifications;

public class DeviceIdentityRecord : EntityBase
{
    [DataMember]
    public string IdentityId { get; set; }

    [Required]
    [DataMember]
    public string DeviceToken { get; set; }

    [Required]
    [DataMember]
    public DeviceType DeviceType { get; set; }
}