using System;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.Events
{
    public class EventFeedbackResponse : ResponseBase
    {
        [DataMember]
        public Guid EventId { get; set; }

        [DataMember]
        public int Rating { get; set; }

        [DataMember]
        public string Message { get; set; }
    }
}