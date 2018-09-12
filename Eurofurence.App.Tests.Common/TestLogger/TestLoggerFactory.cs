using Microsoft.Extensions.Logging;

namespace Eurofurence.App.Tests.Common.TestLogger
{
    public class TestLoggerFactory : ILoggerFactory
    {
        public void AddProvider(ILoggerProvider provider)
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new TestLogger();
        }

        public void Dispose()
        {
        }
    }
}
