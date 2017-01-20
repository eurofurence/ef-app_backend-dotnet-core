using System.Collections.Generic;

namespace Eurofurence.App.Domain.Model
{
    public class EventConferenceRoomRecord : EntityBase
    {
        public string Name { get; set; }

        public virtual ICollection<EventRecord> Events { get; set; }
    }
}