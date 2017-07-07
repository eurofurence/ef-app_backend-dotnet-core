using System.IO;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.AwsCloudWatch;

namespace Eurofurence.App.Server.Web.Extensions
{
    public class JsonLogEventRenderer : ILogEventRenderer
    {
        private readonly JsonFormatter _formatter;

        public JsonLogEventRenderer()
        {
            _formatter = new JsonFormatter();
        }
        public string RenderLogEvent(LogEvent logEvent)
        {
            var stringWriter = new StringWriter();
            _formatter.Format(logEvent, stringWriter);
            return stringWriter.ToString();
        }
    }
}