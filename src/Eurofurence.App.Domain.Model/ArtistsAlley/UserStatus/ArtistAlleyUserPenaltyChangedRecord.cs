using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Eurofurence.App.Domain.Model.ArtistsAlley
{
    /// <summary>
    /// An audit log entry for issued artist alley penalties
    /// </summary>
    [DataContract]
    [Table("ArtistAlleyUserPenaltiesChanges")]
    public class ArtistAlleyUserPenaltyChangedRecord : EntityBase
    {
        /// <summary>
        /// The time a penalty was issued or changed
        /// </summary>
        [DataMember]
        [Required]
        [Column("ChangedDateTimeUtc")]
        public DateTime ChangedDateTimeUtc { get; set; }

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
        /// The old penalty
        /// </summary>
        [DataMember]
        [Required]
        [Column("OldPenalties")]
        public ArtistAlleyUserPenaltyRecord.UserPenalties OldPenalties { get; set; }

        /// <summary>
        /// The new penalty which was set
        /// </summary>
        [DataMember]
        [Required]
        [Column("NewPenalties")]
        public ArtistAlleyUserPenaltyRecord.UserPenalties NewPenalties { get; set; }

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