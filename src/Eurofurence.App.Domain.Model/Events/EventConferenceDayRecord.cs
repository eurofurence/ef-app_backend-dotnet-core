using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.Events
{
    [DataContract]
    public class EventConferenceDayRecord : EntityBase
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public DateTime Date { get; set; }

        [IgnoreDataMember]
        public virtual ICollection<EventRecord> Events { get; set; }
    }
}