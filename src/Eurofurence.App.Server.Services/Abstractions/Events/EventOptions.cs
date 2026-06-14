using System.Collections.Generic;

namespace Eurofurence.App.Server.Services.Abstractions.Events
{
    public class EventOptions
    {
        public string ApiUrl { get; init; }
        public string ApiKey { get; init; }
        public string EventSlug { get; init; }
        public string DefaultLocale { get; init; }
        // Deprecated Option
        public string Url { get; init; }
        // Deprecated Option
        public HashSet<string> InternalTracksLowerCase { get; init; }
    }
}
