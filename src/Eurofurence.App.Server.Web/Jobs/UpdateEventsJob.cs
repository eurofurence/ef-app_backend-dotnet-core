using Eurofurence.App.Server.Services.Abstractions.Dealers;
using Eurofurence.App.Server.Services.Abstractions;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Threading.Tasks;
using System;
using Eurofurence.App.Server.Services.Abstractions.Events;

namespace Eurofurence.App.Server.Web.Jobs
{
    [DisallowConcurrentExecution]
    public class UpdateEventsJob : IJob
    {
        private readonly IEventService _eventService;
        private readonly ILogger _logger;

        public UpdateEventsJob(
            ILoggerFactory loggerFactory,
            IEventService eventService)
        {
            _eventService = eventService;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation(LogEvents.Import, $"Starting job {context.JobDetail.Key.Name}");

            try
            {
                await _eventService.RunImportAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(LogEvents.Import, $"Job {context.JobDetail.Key.Name} failed with exception: {e.Message} {e.StackTrace}");
            }
        }
    }
}
