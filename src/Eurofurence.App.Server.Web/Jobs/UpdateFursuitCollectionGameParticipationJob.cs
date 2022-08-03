using Eurofurence.App.Server.Services.Abstractions.Fursuits;
using FluentScheduler;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Web.Jobs
{
    public class UpdateFursuitCollectionGameParticipationJob : IJob
    {
        private ICollectingGameService _collectingGameService;
        private ILogger _logger;

        public UpdateFursuitCollectionGameParticipationJob(
            ILoggerFactory loggerFactory,
            ICollectingGameService collectingGameService)
        {
            _collectingGameService = collectingGameService;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public void Execute()
        {
            try
            {
                ExecuteAsync().Wait();
            }
            catch (Exception e)
            {
                _logger.LogError("Job failed with exception {Message} {StackTrace}", e.Message, e.StackTrace);
            }
        }

        private async Task ExecuteAsync()
        {
            await _collectingGameService.UpdateFursuitParticipationAsync();
        }
    }
}
