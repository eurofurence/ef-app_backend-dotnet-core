using System;
using System.Collections.Generic;

namespace Eurofurence.App.Domain.Model.Events.Pretalx
{
    public class PretalxSchedule
    {
        public int Id { get; set; }
        public string Version { get; set; }
        public DateTime Published { get; set; }
        public Dictionary<string, string> Comment { get; set; }
        public PretalxSlot[] slots { get; set; }
    }
}
