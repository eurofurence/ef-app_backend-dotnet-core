using System;
using System.Threading.Tasks;
using Eurofurence.App.Server.Services.Abstractions.Communication;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Eurofurence.App.Server.Web.Jobs
{
    public class FlushPrivateMessageNotificationsJob : IJob
    {
        private readonly ILogger _logger;
        private readonly IPrivateMessageService _privateMessageService;

        public FlushPrivateMessageNotificationsJob(
            ILoggerFactory loggerFactory,
            IPrivateMessageService privateMessageService
        )
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _privateMessageService = privateMessageService;
        }

        public Task Execute(IJobExecutionContext context)
        {
            _logger.LogDebug($"Starting job {context.JobDetail.Key.Name}");

            try
            {
                var count = _privateMessageService.FlushPrivateMessageQueueNotifications().Result;
                if (count > 0)
                {
                    _logger.LogInformation($"Flushed {count} messages");
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Job {context.JobDetail.Key.Name} failed with exception: {e.Message} {e.StackTrace}");
            }

            return Task.CompletedTask;
        }
    }
}
