using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.ArtistsAlley
{
    public class ArtistAlleyUserPenaltyResponse : ResponseBase
    {
        /// <summary>
        /// Id of the user from the identity server
        /// </summary>
        [DataMember]
        public string IdentityId { get; set; }

        /// <summary>
        /// The current status of the user
        /// </summary>
        [DataMember]
        public ArtistAlleyUserPenaltyRecord.PenaltyStatus Status { get; set; }
    }
}