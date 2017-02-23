using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Eurofurence.App.Domain.Model.Events
{
    public class EventConferenceRoomRecord : EntityBase
    {
        [DataMember]
        public string Name { get; set; }

        [IgnoreDataMember]
        public virtual ICollection<EventRecord> Events { get; set; }
    }
}