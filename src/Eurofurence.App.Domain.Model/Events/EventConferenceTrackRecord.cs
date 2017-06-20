using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.Events
{
    [DataContract]
    public class EventConferenceTrackRecord : EntityBase
    {
        [DataMember]
        public string Name { get; set; }

        [IgnoreDataMember]
        public virtual ICollection<EventRecord> Events { get; set; }
    }
}