using System.Collections.Generic;

namespace Eurofurence.App.Domain.Model.Events.Pretalx
{
    public class PretalxTag
    {
        public int Id { get; set; }
        public string Tag { get; set; }
        public Dictionary<string, string> Description { get; set; }
        public string Color { get; set; }
        public bool IsPublic { get; set; }

    }
}