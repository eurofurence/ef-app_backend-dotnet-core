using Autofac;
using Microsoft.Extensions.Logging;

namespace Eurofurence.App.Tests.Common.TestLogger
{
    public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<TestLoggerFactory>().As<ILoggerFactory>();
        }
    }
}
