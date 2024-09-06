using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Eurofurence.App.Domain.Model.ArtistsAlley
{

    
    /// <summary>
    /// Record for storing
    /// </summary>
    [Table("ArtistAlleyUserPenalties")]
    public class ArtistAlleyUserPenaltyRecord : EntityBase
    {

        public ArtistAlleyUserPenaltyRecord()
        {
            Penalty = UserPenalties.OK;
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
        [Column("UserId")]
        public string UserId { get; set; }
        
        /// <summary>
        /// The current status of the user
        /// </summary>
        [Column("Penalty")]
        public UserPenalties Penalty { get; set; }


        public virtual List<ArtistAlleyUserPenaltyChangedRecord> AuditLog { get; set; } = new();

    }
}