using System.Collections.Generic;

namespace Eurofurence.App.Domain.Model.Events.Pretalx
{
    /// <summary>
    /// Used for deserialization of the submission type model from the pretalx API.
    /// 
    /// see https://docs.pretalx.org/api/resources/#tag/submission-types/operation/submission_types_retrieve
    /// </summary>
    public class PretalxSubmissionType
    {
        public int Id { get; init; }
        public Dictionary<string, string> Name { get; init; }
        public bool RequireAccessCode { get; init; }
        public bool AttendeeSignupRequired { get; init; }
    }
}