using System.Collections.Generic;

namespace Eurofurence.App.Domain.Model.Events.Pretalx
{
    public class PretalxSubmissionType
    {
        public int Id { get; init; }
        public Dictionary<string, string> Name { get; init; }
        public bool RequireAccessCode { get; init; }
        public bool AttendeeSignupRequired { get; init; }
    }
}