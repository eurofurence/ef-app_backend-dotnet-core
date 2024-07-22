using Eurofurence.App.Server.Services.Abstractions.Fursuits;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Quartz;

namespace Eurofurence.App.Server.Web.Jobs
{
    public class UpdateFursuitCollectionGameParticipationJob : IJob
    {
        private readonly ICollectingGameService _collectingGameService;
        private readonly ILogger _logger;

        public UpdateFursuitCollectionGameParticipationJob(
            ILoggerFactory loggerFactory,
            ICollectingGameService collectingGameService)
        {
            _collectingGameService = collectingGameService;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation($"Starting job {context.JobDetail.Key.Name}");

            try
            {
                await _collectingGameService.UpdateFursuitParticipationAsync();
            }
            catch (Exception e)
            {
                _logger.LogError($"Job {context.JobDetail.Key.Name} failed with exception {e.Message} {e.StackTrace}");
            }
        }
    }
}
