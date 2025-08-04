using System;
using System.Runtime.Serialization;
using Eurofurence.App.Domain.Model.Transformers;

namespace Eurofurence.App.Domain.Model.Events
{
    [DataContract]
    public class EventFeedbackRecord : EntityBase, IDtoTransformable<EventFeedbackResponse>
    {
        [DataMember]
        public Guid EventId { get; set; }

        public EventRecord Event { get; set; }

        [DataMember]
        public int Rating { get; set; }

        [DataMember]
        public string Message { get; set; }
    }
}