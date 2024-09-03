using System.Collections.Generic;

namespace Eurofurence.App.Domain.Model.ArtistsAlley
{

    
    /// <summary>
    /// Record for storing
    /// </summary>
    public class ArtistAlleyUserStatusRecord : EntityBase
    {

        public ArtistAlleyUserStatusRecord()
        {
            Status = UserStatus.OK;
        }

        /// <summary>
        /// Possible status of an user
        /// </summary>
        public enum UserStatus
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
        public UserStatus Status { get; set; } = UserStatus.OK;


        public virtual List<ArtistAlleyUserStatusChangedRecord> AuditLog { get; set; } = new();

    }
}