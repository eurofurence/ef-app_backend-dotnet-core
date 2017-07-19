using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.ArtShow;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.ArtShow;

namespace Eurofurence.App.Server.Services.ArtShow
{
    public class ItemActivityService :IItemActivityService
    {
        private readonly ConventionSettings _conventionSettings;
        private readonly IEntityRepository<ItemActivityRecord> _itemActivityRepository;

        public ItemActivityService(
            ConventionSettings conventionSettings, 
            IEntityRepository<ItemActivityRecord> itemActivityRepository)
        {
            _conventionSettings = conventionSettings;
            _itemActivityRepository = itemActivityRepository;
        }

        public async Task ImportActivityLogAsync(TextReader logReader)
        {
            var csv = new CsvReader(logReader);

            csv.Configuration.RegisterClassMap<LogImportRowClassMap>();
            csv.Configuration.Delimiter = ",";
            csv.Configuration.HasHeaderRecord = false;

            var csvRecords = csv.GetRecords<LogImportRow>().ToList();
            
            foreach (var csvRecord in csvRecords)
            {
                var existingRecord = await _itemActivityRepository.FindOneAsync(a => a.ImportHash == csvRecord.Hash.Value);
                if (existingRecord != null) continue;

                var newRecord = new ItemActivityRecord()
                {
                    Id = Guid.NewGuid(),
                    OwnerUid = $"RegSys:{_conventionSettings.ConventionNumber}:{csvRecord.RegNo}",
                    ASIDNO = csvRecord.ASIDNO,
                    ArtistName = csvRecord.ArtistName,
                    ArtPieceTitle = csvRecord.ArtPieceTitle,
                    Status = (ItemActivityRecord.StatusEnum) Enum.Parse(typeof(ItemActivityRecord.StatusEnum),
                        csvRecord.Status, true),
                    ImportDateTimeUtc = DateTime.UtcNow,
                    ImportHash = csvRecord.Hash.Value
                };
                newRecord.Touch();

                await _itemActivityRepository.InsertOneAsync(newRecord);
            }
        }
    }
}
