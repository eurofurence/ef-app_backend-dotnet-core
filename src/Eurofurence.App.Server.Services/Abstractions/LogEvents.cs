using Microsoft.Extensions.Logging;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public static class LogEvents
    {
        public static EventId Audit = new EventId(1, "Audit");
    }
}
