using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Eurofurence.App.Domain.Model.ArtistsAlley
{
    [DataContract]
    public class ArtistAlleyUserStatusChangedRecord : EntityBase
    {
        [DataMember]
        [Required]
        public DateTime ChangedDateTimeUtc { get; set; }

        [DataMember]
        [Required]
        public string ChangedBy { get; set; }

        [DataMember]
        [Required]
        public ArtistAlleyUserStatusRecord.UserStatus OldStatus { get; set; }

        [DataMember]
        [Required]
        public ArtistAlleyUserStatusRecord.UserStatus NewStatus { get; set; }
        
        [DataMember]
        public string Reason { get; set; }

        [DataMember]
        public Guid UserStatusRecordID { get; set; }

        [JsonIgnore]
        public ArtistAlleyUserStatusRecord UserStatusRecord { get; set; }
    }
}