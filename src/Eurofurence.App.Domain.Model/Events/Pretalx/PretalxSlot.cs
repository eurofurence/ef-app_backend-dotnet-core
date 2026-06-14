using System;

namespace Eurofurence.App.Domain.Model.Events.Pretalx
{
    public class PretalxSlot
    {
        public int Id { get; set; }
        public PretalxRoom Room { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public PretalxSubmission Submission { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; }
    }
}