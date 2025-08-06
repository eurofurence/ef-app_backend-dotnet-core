using Eurofurence.App.Domain.Model.ArtistsAlley;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.ArtistsAlley;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Sentry;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Eurofurence.App.Server.Web.Jobs
{
    [DisallowConcurrentExecution]
    public class DeleteExpiredArtistAlleyRegistrationsJob : IJob
    {
        private readonly ITableRegistrationService _tableRegistrationService;
        private readonly ILogger _logger;
        private readonly ArtistAlleyOptions _options;

        public DeleteExpiredArtistAlleyRegistrationsJob(
            ILoggerFactory loggerFactory,
            ITableRegistrationService tableRegistrationService,
            IOptions<ArtistAlleyOptions> options)
        {
            _tableRegistrationService = tableRegistrationService;
            _logger = loggerFactory.CreateLogger(GetType());
            _options = options.Value;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation(LogEvents.Audit, $"Starting job {context.JobDetail.Key.Name}");

            try
            {
                if (_options.ExpirationTimeInHours == null)
                {
                    _logger.LogWarning(LogEvents.Audit,
                        $"Artist alley ExpirationTimeInHours is not configured. Artist alley registrations will not expire.");
                    return;
                }

                var expiredRegistrations = (await _tableRegistrationService
                        .GetRegistrations(TableRegistrationRecord.RegistrationStateEnum.Pending)
                        .ToListAsync())
                    .Where(r => (DateTime.UtcNow - r.CreatedDateTimeUtc).TotalHours > _options.ExpirationTimeInHours);

                foreach (var registration in expiredRegistrations)
                {
                    await _tableRegistrationService.DeleteOneAsync(registration.Id);
                    _logger.LogInformation(LogEvents.Audit,
                        $"Artist alley registration with the ID {registration.Id} of user {registration.OwnerUsername} expired and was deleted.");
                }
            }
            catch (Exception e)
            {
                SentrySdk.CaptureException(e);
                _logger.LogError(LogEvents.Audit,
                    $"Job {context.JobDetail.Key.Name} failed with exception: {e.Message} {e.StackTrace}");
            }
        }
    }
}