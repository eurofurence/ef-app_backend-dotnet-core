using Serilog.Events;
using Serilog.Sinks.AwsCloudWatch;

namespace Eurofurence.App.Server.Web.Extensions
{
    public class CustomLogEventRenderer : ILogEventRenderer
    {
        public string RenderLogEvent(LogEvent logEvent)
        {
            return $"[{logEvent.Level}] ({logEvent.Properties["SourceContext"]}) {logEvent.RenderMessage()}";
        }
    }
}