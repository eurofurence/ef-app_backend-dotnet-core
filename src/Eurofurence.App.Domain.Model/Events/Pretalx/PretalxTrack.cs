using System.Collections.Generic;

namespace Eurofurence.App.Domain.Model.Events.Pretalx
{
    public class PretalxTrack
    {
        public int Id { get; set; }
        public Dictionary<string, string> Name { get; set; }
        public Dictionary<string, string> Description { get; set; }
        public string Color { get; set; }
        public int Position { get; set; }
        public bool RequireAccessCode { get; set; }
        public bool AttendeeSignupRequired { get; set; }
    }
}