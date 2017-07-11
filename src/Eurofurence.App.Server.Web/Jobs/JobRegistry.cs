using System;
using FluentScheduler;
using Microsoft.Extensions.Configuration;

namespace Eurofurence.App.Server.Web.Jobs
{
    public class JobRegistry : Registry
    {
        public JobRegistry(IConfiguration configuration)
        {
            Schedule<UpdateNewsJob>().ToRunNow().AndEvery(Convert.ToInt32(configuration["updateNews:secondsInterval"])).Seconds();
        }
    }
}
