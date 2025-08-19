using System.Collections.Generic;

namespace Eurofurence.App.Server.Services.Abstractions.Events
{
    public class EventOptions
    {
        public string Url { get; init; }
        public HashSet<string> InternalTracksLowerCase { get; init; }
    }
}
