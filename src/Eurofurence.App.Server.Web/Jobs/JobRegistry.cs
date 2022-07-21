using System;
using FluentScheduler;
using Microsoft.Extensions.Configuration;

namespace Eurofurence.App.Server.Web.Jobs
{
    public class JobRegistry : Registry
    {
        public JobRegistry(IConfiguration configuration)
        {
            NonReentrantAsDefault();

            Schedule<FlushPrivateMessageNotificationsJob>().ToRunEvery(1).Seconds();
            Schedule<UpdateNewsJob>().ToRunNow().AndEvery(Convert.ToInt32(configuration["updateNews:secondsInterval"])).Seconds();
            Schedule<UpdateLostAndFoundJob>().ToRunNow().AndEvery(60).Seconds();
        }
    }
}
