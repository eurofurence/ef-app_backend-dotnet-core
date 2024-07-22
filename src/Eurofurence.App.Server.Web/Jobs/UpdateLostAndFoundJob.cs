using Eurofurence.App.Server.Services.Abstractions.LostAndFound;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Quartz;

namespace Eurofurence.App.Server.Web.Jobs
{
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
            _logger.LogInformation($"Starting job {context.JobDetail.Key.Name}");

            try
            {
                await _lostAndFoundLassieImporter.RunImportAsync();
            }
            catch (Exception e)
            {
                _logger.LogError($"Job {context.JobDetail.Key.Name} failed with exception {e.Message} {e.StackTrace}");
            }
        }
    }
}
