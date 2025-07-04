using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Eurofurence.App.Server.Web.Controllers.Transformers;

namespace Eurofurence.App.Domain.Model.PushNotifications;

public class DeviceIdentityRecord : EntityBase, IDtoTransformable<DeviceIdentityResponse>
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