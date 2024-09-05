using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.ArtistsAlley
{
    [DataContract]
    public class ArtistAlleyUserPenaltyChangeRequest
    {

        /// <summary>
        /// The penalty which should be set for the user
        /// </summary>
        [DataMember]
        [Required]
        public ArtistAlleyUserPenaltyRecord.UserPenalties Penalties { get; set; }

        /// <summary>
        /// An optional reason why the penalty was issued or revoked.
        /// </summary>
        [DataMember]
        public string Reason { get; set; } = "";
    }
}