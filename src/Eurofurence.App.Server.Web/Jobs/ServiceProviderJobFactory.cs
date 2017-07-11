using System;
using FluentScheduler;
using Microsoft.Extensions.DependencyInjection;

namespace Eurofurence.App.Server.Web.Jobs
{
    public class ServiceProviderJobFactory :IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceProviderJobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IJob GetJobInstance<T>() where T : IJob
        {
            return _serviceProvider.GetService<T>();
        }
    }
}
