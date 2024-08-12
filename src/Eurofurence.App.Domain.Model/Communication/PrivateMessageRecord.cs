using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.Communication
{
    [DataContract]
    public class PrivateMessageRecord : EntityBase
    {
        [DataMember]
        public string RecipientRegSysId { get; set; }
        
        [DataMember]
        public string RecipientIdentityId { get; set; }

        [DataMember]
        public string SenderUid { get; set; }

        [DataMember]
        [Required]
        public DateTime CreatedDateTimeUtc { get; set; }

        [DataMember]
        public DateTime? ReceivedDateTimeUtc { get; set; }

        [DataMember]
        public DateTime? ReadDateTimeUtc { get; set; }

        [DataMember]
        public string AuthorName { get; set; }

        [DataMember]
        public string Subject { get; set; }

        [DataMember]
        public string Message { get; set; }
    }
}