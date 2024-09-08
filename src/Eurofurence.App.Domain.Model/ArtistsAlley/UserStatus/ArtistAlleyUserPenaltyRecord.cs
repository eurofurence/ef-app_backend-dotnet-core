using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

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
            Status = PenaltyStatus.OK;
        }

        /// <summary>
        /// Possible penalties of a user
        /// </summary>
        public enum PenaltyStatus
        {
            OK = 0,
            BANNED = 1,
        }

        /// <summary>
        /// Id of the user from the identity server
        /// </summary>
        [Column("IdentityId")]
        public string IdentityId { get; set; }

        /// <summary>
        /// The current status of the user
        /// </summary>
        [Column("Status")]
        public PenaltyStatus Status { get; set; }


        public virtual List<StateChangeRecord> AuditLog { get; set; } = new();

        /// <summary>
        /// An audit log entry for issued artist alley penalties
        /// </summary>
        [DataContract]
        [Table("ArtistAlleyUserPenaltyChangeRecord")]
        public class StateChangeRecord : EntityBase
        {

            /// <summary>
            /// By whom the penalty was changed.
            ///
            /// This will most likely be the username
            /// </summary>
            [DataMember]
            [Required]
            [Column("ChangedBy")]
            public string ChangedBy { get; set; }
        
            /// <summary>
            /// The new penalty which was set
            /// </summary>
            [DataMember]
            [Required]
            [Column("PenaltyStatus")]
            public ArtistAlleyUserPenaltyRecord.PenaltyStatus PenaltyStatus { get; set; }

            /// <summary>
            /// An optional reason, why the penalty was issued or revoked
            /// </summary>
            [DataMember]
            [Column("Reason")]
            public string Reason { get; set; }

            /// <summary>
            /// The FK of the actual user penalty record
            /// </summary>
            [DataMember]
            [Column("UserPenaltyRecordId")]
            [ForeignKey(nameof(UserPenaltyRecord))]
            public Guid UserPenaltyRecordId { get; set; }

            /// <summary>
            /// Reference to the <see cref="UserPenaltyRecord"/>
            /// </summary>
            [JsonIgnore]
            public ArtistAlleyUserPenaltyRecord UserPenaltyRecord { get; set; }
        }
    }
}