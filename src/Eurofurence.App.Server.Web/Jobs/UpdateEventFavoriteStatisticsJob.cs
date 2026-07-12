using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Events;
using Microsoft.Extensions.Logging;
using Quartz;
using Sentry;
using System;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Web.Jobs
{
    [DisallowConcurrentExecution]
    public class UpdateEventFavoriteStatisticsJob : IJob
    {
        private readonly IEventFavoriteStatisticsService _eventFavoriteStatisticsService;
        private readonly ILogger _logger;

        public UpdateEventFavoriteStatisticsJob(
            ILoggerFactory loggerFactory,
            IEventFavoriteStatisticsService eventFavoriteStatisticsService)
        {
            _eventFavoriteStatisticsService = eventFavoriteStatisticsService;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation(LogEvents.Import, $"Starting job {context.JobDetail.Key.Name}");

            try
            {
                await _eventFavoriteStatisticsService.InsertForAllStartedEvents();
            }
            catch (Exception e)
            {
                SentrySdk.CaptureException(e);
                _logger.LogError(LogEvents.Import, $"Job {context.JobDetail.Key.Name} failed with exception: {e.Message} {e.StackTrace}");
            }
        }
    }
}
