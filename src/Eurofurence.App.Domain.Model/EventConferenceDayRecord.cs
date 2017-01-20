using System;
using System.Collections.Generic;

namespace Eurofurence.App.Domain.Model
{
    public class EventConferenceDayRecord : EntityBase
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }

        public virtual ICollection<EventRecord> Events { get; set; }
    }
}