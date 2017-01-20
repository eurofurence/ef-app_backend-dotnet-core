using System.Collections.Generic;

namespace Eurofurence.App.Domain.Model
{
    public class EventConferenceTrackRecord : EntityBase
    {
        public string Name { get; set; }

        public virtual ICollection<EventRecord>  Events { get; set; }
    }
}