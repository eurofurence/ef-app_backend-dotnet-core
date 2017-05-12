using Serilog.Sinks.AwsCloudWatch;
using Serilog.Events;

namespace Eurofurence.App.Server.Web.Extensions
{
    public class CustomLogEventRenderer : ILogEventRenderer
    {
        public string RenderLogEvent(LogEvent logEvent)
        {
            return $"[{logEvent.Level}] {logEvent.RenderMessage()}";
        }
    }
}
