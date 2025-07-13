using Eurofurence.App.Server.Services.Abstractions.LostAndFound;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Quartz;
using Eurofurence.App.Server.Services.Abstractions;
using System.Threading;
using Sentry;

namespace Eurofurence.App.Server.Web.Jobs
{
    [DisallowConcurrentExecution]
    public class UpdateLostAndFoundJob : IJob
    {
        private readonly ILostAndFoundLassieImporter _lostAndFoundLassieImporter;
        private readonly ILogger _logger;

        public UpdateLostAndFoundJob(
            ILoggerFactory loggerFactory,
            ILostAndFoundLassieImporter lostAndFoundLassieImporter)
        {
            _lostAndFoundLassieImporter = lostAndFoundLassieImporter;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation(LogEvents.Import, $"Starting job {context.JobDetail.Key.Name}");

            try
            {
                await _lostAndFoundLassieImporter.RunImportAsync();
            }
            catch (Exception e)
            {
                SentrySdk.CaptureException(e);
                _logger.LogError(LogEvents.Import, $"Job {context.JobDetail.Key.Name} failed with exception: {e.Message} {e.StackTrace}");
            }
        }
    }
}
