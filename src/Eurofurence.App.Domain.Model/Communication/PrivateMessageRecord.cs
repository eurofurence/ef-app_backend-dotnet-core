using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Eurofurence.App.Domain.Model.Transformers;
using Eurofurence.App.Server.Web.Controllers.Transformers;
using Mapster;

namespace Eurofurence.App.Domain.Model.Communication
{
    [DataContract]
    public class PrivateMessageRecord : EntityBase,
        IDtoRecordTransformable<SendPrivateMessageByRegSysRequest, PrivateMessageResponse, PrivateMessageRecord>,
        IDtoRecordTransformable<SendPrivateMessageByIdentityRequest, PrivateMessageResponse, PrivateMessageRecord>
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