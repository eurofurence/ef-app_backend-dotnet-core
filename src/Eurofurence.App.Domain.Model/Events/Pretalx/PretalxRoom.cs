using System;
using System.Collections.Generic;

namespace Eurofurence.App.Domain.Model.Events.Pretalx
{
    public class PretalxRoom
    {
        public int Id { get; set; }
        public Dictionary<string, string> Name { get; set; }
        public Dictionary<string, string> Description { get; set; }
        public Guid Uuid { get; set; }
        public int Capacity { get; set; }
        public int Position { get; set; }
    }
}