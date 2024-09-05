using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Eurofurence.App.Domain.Model.ArtistsAlley
{
    [DataContract]
    public class ArtistAlleyUserPenaltyChangedRecord : EntityBase
    {
        [DataMember]
        [Required]
        public DateTime ChangedDateTimeUtc { get; set; }

        [DataMember]
        [Required]
        public string ChangedBy { get; set; }

        [DataMember]
        [Required]
        public ArtistAlleyUserPenaltyRecord.UserPenalties OldPenalties { get; set; }

        [DataMember]
        [Required]
        public ArtistAlleyUserPenaltyRecord.UserPenalties NewPenalties { get; set; }
        
        [DataMember]
        public string Reason { get; set; }

        [DataMember]
        public Guid UserPenaltyRecordId { get; set; }

        [JsonIgnore]
        public ArtistAlleyUserPenaltyRecord UserPenaltyRecord { get; set; }
    }
}