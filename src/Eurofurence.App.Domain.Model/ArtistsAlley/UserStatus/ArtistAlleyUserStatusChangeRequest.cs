using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.ArtistsAlley
{
    [DataContract]
    public class ArtistAlleyUserStatusChangeRequest
    {
        
        [DataMember]
        [Required]
        public ArtistAlleyUserStatusRecord.UserStatus Status { get; set; }
        
        [DataMember]
        public string Reason { get; set; }
    }
}