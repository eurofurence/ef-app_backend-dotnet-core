using System.Collections.Generic;

namespace Eurofurence.App.Domain.Model.Events.Pretalx
{
    public class PretalxSubmissionType
    {
        public int Id { get; set; }
        public Dictionary<string, string> Name { get; set; }
        public bool RequireAccessCode { get; set; }
        public bool AttendeeSignupRequired { get; set; }
    }
}