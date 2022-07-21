using Eurofurence.App.Server.Services.Abstractions.LostAndFound;
using FluentScheduler;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Web.Jobs
{
    public class UpdateLostAndFoundJob : IJob
    {
        private ILostAndFoundLassieImporter _lostAndFoundLassieImporter;
        private ILogger _logger;

        public UpdateLostAndFoundJob(
            ILoggerFactory loggerFactory,
            ILostAndFoundLassieImporter lostAndFoundLassieImporter)
        {
            _lostAndFoundLassieImporter = lostAndFoundLassieImporter;
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
            await _lostAndFoundLassieImporter.RunImportAsync();
        }
    }
}
