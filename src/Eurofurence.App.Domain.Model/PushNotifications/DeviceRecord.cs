using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.PushNotifications;

public class DeviceRecord : EntityBase
{
    [Required]
    [DataMember]
    public string IdentityId { get; set; }

    [DataMember]
    public string RegSysId { get; set; }

    [Required]
    [DataMember]
    public string DeviceToken { get; set; }

    [Required]
    [DataMember]
    public bool IsAndroid { get; set; }
}