using Eurofurence.App.Server.Services.Abstractions.Communication;
using FluentScheduler;
using Microsoft.Extensions.Logging;

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

        public void Execute()
        {
            var count = _privateMessageService.FlushPrivateMessageQueueNotifications().Result;
            if (count == 0) return;

            _logger.LogInformation($"Flushed {count} messages");
        }
    }
}
