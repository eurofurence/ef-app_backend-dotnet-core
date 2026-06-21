using System;
using System.Collections.Generic;

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
    }
}