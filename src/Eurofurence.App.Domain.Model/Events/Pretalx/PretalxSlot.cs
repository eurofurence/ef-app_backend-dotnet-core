using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Eurofurence.App.Domain.Model.Events.Pretalx
{
    /// <summary>
    /// Used for deserialization of the slot model from the pretalx API.
    /// 
    /// see https://docs.pretalx.org/api/resources/#tag/slots/operation/slots_retrieve
    /// </summary>
    public class PretalxSlot
    {
        public int Id { get; init; }
        public PretalxRoom Room { get; init; }
        public DateTime? Start { get; init; }
        public DateTime? End { get; init; }
        public PretalxSubmission Submission { get; init; }
        public Dictionary<string, string> Description { get; init; }
        public int Duration { get; init; }
        /// <summary>
        /// Artifical ID derived from either <c>Submission.Code</c> or the first six digits of an
        /// MD5 hash including <c>Room</c>, <c>Start</c> and <c>End</c> plus an index of the six
        /// digit code occurring within the schedule (e.g. if a submission has multiple slots).
        /// </summary>
        [JsonIgnore]
        public string SourceId { get; set; }
    }
}