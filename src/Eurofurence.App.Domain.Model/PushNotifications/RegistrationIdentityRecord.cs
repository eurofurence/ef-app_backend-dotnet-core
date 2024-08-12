using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.PushNotifications;

public class RegistrationIdentityRecord : EntityBase
{
    [Required]
    [DataMember]
    public string RegSysId { get; set; }

    [Required]
    [DataMember]
    public string IdentityId { get; set; }
}