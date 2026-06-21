using System.Collections.Generic;

namespace Eurofurence.App.Server.Services.Abstractions.Events
{
    public class EventOptions
    {
        public string ApiUrl { get; init; }
        public string ApiKey { get; init; }
        public string EventSlug { get; init; }
        public string DefaultLocale { get; init; }
        public int InternalTagId { get; init; }
        public int AcceptsFeedbackTagId { get; init; }
        public Dictionary<string, string> EventDays { get; init; }
    }
}
