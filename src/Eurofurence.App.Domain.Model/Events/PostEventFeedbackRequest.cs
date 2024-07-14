using System;

namespace Eurofurence.App.Domain.Model.Events
{
    public class PostEventFeedbackRequest
    {
        public Guid EventId { get; set; }
        public int Rating { get; set; }
        public string Message { get; set; }
    }
}
