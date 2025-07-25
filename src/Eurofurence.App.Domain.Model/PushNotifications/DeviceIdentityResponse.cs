#nullable enable
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.PushNotifications
{
    public class DeviceIdentityResponse : ResponseBase
    {
        [DataMember]
        public string? IdentityId { get; set; }

        [Required]
        [DataMember]
        public required string DeviceToken { get; set; }

        [Required]
        [DataMember]
        public DeviceType DeviceType { get; set; }
    }
}