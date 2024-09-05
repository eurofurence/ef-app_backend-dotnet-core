using System.Collections.Generic;

namespace Eurofurence.App.Domain.Model.ArtistsAlley
{

    
    /// <summary>
    /// Record for storing
    /// </summary>
    public class ArtistAlleyUserPenaltyRecord : EntityBase
    {

        public ArtistAlleyUserPenaltyRecord()
        {
            Penalties = UserPenalties.OK;
        }

        /// <summary>
        /// Possible penalties of a user
        /// </summary>
        public enum UserPenalties
        {
            BANNED = 0,
            OK = 1
        }

        /// <summary>
        /// Id of the user from the reg system
        /// </summary>
        public string UserId { get; set; }
        
        /// <summary>
        /// The current status of the user
        /// </summary>
        public UserPenalties Penalties { get; set; }


        public virtual List<ArtistAlleyUserPenaltyChangedRecord> AuditLog { get; set; } = new();

    }
}