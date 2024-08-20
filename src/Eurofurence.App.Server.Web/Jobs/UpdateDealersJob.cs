using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System;
using Eurofurence.App.Server.Services.Abstractions.Dealers;
using Quartz;
using Eurofurence.App.Server.Services.Abstractions;

namespace Eurofurence.App.Server.Web.Jobs
{
    [DisallowConcurrentExecution]
    public class UpdateDealersJob : IJob
    {
        private readonly IDealerService _dealerService;
        private readonly ILogger _logger;

        public UpdateDealersJob(
            ILoggerFactory loggerFactory,
            IDealerService dealerService)
        {
            _dealerService = dealerService;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation(LogEvents.Import, $"Starting job {context.JobDetail.Key.Name}");

            try
            {
                await _dealerService.RunImportAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(LogEvents.Import, $"Job {context.JobDetail.Key.Name} failed with exception: {e.Message} {e.StackTrace}");
            }
        }
    }
}
