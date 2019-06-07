using System;
using System.Collections.Generic;
using System.Text;

namespace Eurofurence.App.Server.Services.Abstractions.Events
{
    public class PostEventFeedbackRequest
    {
        public Guid EventId { get; set; }
        public int Rating { get; set; }
        public string Message { get; set; }
    }
}
