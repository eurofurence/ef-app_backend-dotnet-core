using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.ArtistsAlley
{
    /// <summary>
    /// An API request for changing the global status of the artist alley
    /// </summary>
    [DataContract]
    public class ArtistAlleyUserPenaltyChangeRequest
    {

        /// <summary>
        /// The penalty which should be set for the user
        /// </summary>
        [DataMember]
        [Required]
        public ArtistAlleyUserPenaltyRecord.PenaltyStatus Penalties { get; set; }

        /// <summary>
        /// An optional reason why the penalty was issued or revoked.
        /// </summary>
        [DataMember]
        public string Reason { get; set; } = "";
    }
}