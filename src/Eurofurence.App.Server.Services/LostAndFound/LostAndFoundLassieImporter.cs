using Eurofurence.App.Common.DataDiffUtils;
using Eurofurence.App.Domain.Model.LostAndFound;
using Eurofurence.App.Server.Services.Abstractions.Lassie;
using Eurofurence.App.Server.Services.Abstractions.LostAndFound;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.LostAndFound
{
    public class LostAndFoundLassieImporter : ILostAndFoundLassieImporter
    {
        private ILostAndFoundService _lostAndFoundService;
        private ILassieApiClient _lassieApiClient;

        public LostAndFoundLassieImporter(
            ILostAndFoundService lostAndFoundService,
            ILassieApiClient lassieApiClient
            )
        {
            _lostAndFoundService = lostAndFoundService;
            _lassieApiClient = lassieApiClient;
        }

        public async Task RunImportAsync()
        {
            var existingRecords = await _lostAndFoundService.FindAllAsync();
            var newRecords = await _lassieApiClient.QueryLostAndFoundDbAsync();

            var patch = new PatchDefinition<LostAndFoundResponse, LostAndFoundRecord>((source, list) =>
                list.SingleOrDefault(a => a.ExternalId == source.Id));

            var statusConverter = new Func<string, LostAndFoundRecord.LostAndFoundStatusEnum>(input =>
            {
                switch(input.ToLowerInvariant())
                {
                    case "l": return LostAndFoundRecord.LostAndFoundStatusEnum.Lost;
                    case "f": return LostAndFoundRecord.LostAndFoundStatusEnum.Found;
                    case "r": return LostAndFoundRecord.LostAndFoundStatusEnum.Returned;
                    default: return LostAndFoundRecord.LostAndFoundStatusEnum.Unknown;
                }
            });

            patch
                .Map(s => s.Id, t => t.ExternalId)
                .Map(s => s.Title, t => t.Title)
                .Map(s => s.Description, t => t.Description)
                .Map(s => s.ImageUrl, t => t.ImageUrl)
                .Map(s => statusConverter(s.Status), t => t.Status)
                .Map(s => s.LostDateTimeLocal?.ToUniversalTime(), t => t.LostDateTimeUtc)
                .Map(s => s.ReturnDateTimeLocal?.ToUniversalTime(), t => t.ReturnDateTimeUtc)
                .Map(s => s.FoundDateTimeLocal?.ToUniversalTime(), t => t.FoundDateTimeUtc);

            var patchResult = patch.Patch(newRecords, existingRecords);

            await _lostAndFoundService.ApplyPatchOperationAsync(patchResult);
        }
    }
}
