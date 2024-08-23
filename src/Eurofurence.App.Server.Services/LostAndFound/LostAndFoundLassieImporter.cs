using Eurofurence.App.Common.DataDiffUtils;
using Eurofurence.App.Domain.Model.LostAndFound;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Lassie;
using Eurofurence.App.Server.Services.Abstractions.LostAndFound;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace Eurofurence.App.Server.Services.LostAndFound
{
    public class LostAndFoundLassieImporter : ILostAndFoundLassieImporter
    {
        private readonly ILostAndFoundService _lostAndFoundService;
        private readonly ILassieApiClient _lassieApiClient;
        private readonly ILogger _logger;
        private static SemaphoreSlim _semaphore = new(1, 1);

        public LostAndFoundLassieImporter(
            ILostAndFoundService lostAndFoundService,
            ILassieApiClient lassieApiClient,
            ILoggerFactory loggerFactory
            )
        {
            _lostAndFoundService = lostAndFoundService;
            _lassieApiClient = lassieApiClient;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public async Task RunImportAsync()
        {
            try
            {
                await _semaphore.WaitAsync();
                _logger.LogInformation(LogEvents.Import, "Starting LostAndFound import.");

                var existingRecords = _lostAndFoundService.FindAll();
                var newRecords = await _lassieApiClient.QueryLostAndFoundDbAsync();

                var patch = new PatchDefinition<LostAndFoundResponse, LostAndFoundRecord>((source, list) =>
                    list.SingleOrDefault(a => a.ExternalId == source.Id));

                var statusConverter = new Func<string, LostAndFoundRecord.LostAndFoundStatusEnum>(input =>
                {
                    switch (input.ToLowerInvariant())
                    {
                        case "l": return LostAndFoundRecord.LostAndFoundStatusEnum.Lost;
                        case "f": return LostAndFoundRecord.LostAndFoundStatusEnum.Found;
                        case "r": return LostAndFoundRecord.LostAndFoundStatusEnum.Returned;
                        default: return LostAndFoundRecord.LostAndFoundStatusEnum.Unknown;
                    }
                });

                patch
                    .Map(s => s.Id, t => t.ExternalId)
                    .Map(s => HttpUtility.HtmlDecode(s.Title), t => t.Title)
                    .Map(s => HttpUtility.HtmlDecode(s.Description), t => t.Description)
                    .Map(s => s.ImageUrl, t => t.ImageUrl)
                    .Map(s => statusConverter(s.Status), t => t.Status)
                    .Map(s => s.LostDateTimeLocal.HasValue ? DateTime.SpecifyKind(s.LostDateTimeLocal.Value, DateTimeKind.Utc).AddHours(-2) : s.LostDateTimeLocal, t => t.LostDateTimeUtc)
                    .Map(s => s.ReturnDateTimeLocal.HasValue ? DateTime.SpecifyKind(s.ReturnDateTimeLocal.Value, DateTimeKind.Utc).AddHours(-2) : s.ReturnDateTimeLocal, t => t.ReturnDateTimeUtc)
                    .Map(s => s.FoundDateTimeLocal.HasValue ? DateTime.SpecifyKind(s.FoundDateTimeLocal.Value, DateTimeKind.Utc).AddHours(-2) : s.FoundDateTimeLocal, t => t.FoundDateTimeUtc);

                var patchResult = patch.Patch(newRecords, existingRecords);

                await _lostAndFoundService.ApplyPatchOperationAsync(patchResult);

                _logger.LogInformation(LogEvents.Import, "LostAndFound import finished successfully.");
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
