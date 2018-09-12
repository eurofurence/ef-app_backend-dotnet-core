using Autofac;
using Eurofurence.App.Domain.Model.Abstractions;

namespace Eurofurence.App.Tests.Common.InMemoryRepository
{
    public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterGeneric(typeof(InMemoryEntityRepository<>))
                .As(typeof(IEntityRepository<>))
                .SingleInstance();
        }
    }
}
