using Microsoft.Extensions.Logging;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public static class LogEvents
    {
        public static EventId Audit = new(1, "Audit");
        public static EventId CollectionGame = new(2, "CollectionGame");
        public static EventId Import = new(3, "Import");
    }
}
