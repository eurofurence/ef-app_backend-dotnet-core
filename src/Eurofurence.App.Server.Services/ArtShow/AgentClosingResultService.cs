using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using Eurofurence.App.Domain.Model.ArtShow;
using Eurofurence.App.Domain.Model.Communication;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.ArtShow;
using Eurofurence.App.Server.Services.Abstractions.Communication;
using Microsoft.EntityFrameworkCore;

namespace Eurofurence.App.Server.Services.ArtShow
{
    public class AgentClosingResultService : IAgentClosingResultService
    {
        private readonly AppDbContext _appDbContext;
        private readonly ConventionSettings _conventionSettings;
        private readonly IPrivateMessageService _privateMessageService;
        private static SemaphoreSlim _semaphore = new(1, 1);

        public AgentClosingResultService(
            AppDbContext appDbContext,
            ConventionSettings conventionSettings,
            IPrivateMessageService privateMessageService
            )
        {
            _appDbContext = appDbContext;
            _conventionSettings = conventionSettings;
            _privateMessageService = privateMessageService;
        }

        public async Task<ImportResult> ImportAgentClosingResultLogAsync(TextReader logReader)
        {
            var importResult = new ImportResult();

            try
            {
                await _semaphore.WaitAsync();
                
                var csv = new CsvReader(logReader, CultureInfo.CurrentCulture);

                csv.Context.RegisterClassMap<AgentClosingResultImportRowClassMap>();
                csv.Context.Configuration.Delimiter = ",";
                csv.Context.Configuration.HasHeaderRecord = false;

                var csvRecords = await csv.GetRecordsAsync<AgentClosingResultImportRow>().ToListAsync();

                foreach (var csvRecord in csvRecords)
                {
                    var existingRecord = await _appDbContext.AgentClosingResults.AsNoTracking().FirstOrDefaultAsync(a => a.ImportHash == csvRecord.Hash.Value);
                    if (existingRecord != null)
                    {
                        importResult.RowsSkippedAsDuplicate++;
                        continue;
                    }

                    var newRecord = new AgentClosingResultRecord()
                    {
                        Id = Guid.NewGuid(),
                        OwnerUid = $"RegSys:{_conventionSettings.ConventionIdentifier}:{csvRecord.AgentBadgeNo}",
                        AgentBadgeNo = csvRecord.AgentBadgeNo,
                        AgentName = csvRecord.AgentName,
                        ArtistName = csvRecord.ArtistName,
                        TotalCashAmount = csvRecord.TotalCashAmount,
                        ExhibitsSold = csvRecord.ExhibitsSold,
                        ExhibitsUnsold = csvRecord.ExhibitsUnsold,
                        ExhibitsToAuction = csvRecord.ExhibitsToAuction,
                        ImportDateTimeUtc = DateTime.UtcNow,
                        ImportHash = csvRecord.Hash.Value
                    };
                    newRecord.Touch();

                    _appDbContext.AgentClosingResults.Add(newRecord);
                    importResult.RowsImported++;
                    await _appDbContext.SaveChangesAsync();
                }
            }
            finally
            {
                _semaphore.Release();
            }

            return importResult;
        }

        private IQueryable<AgentClosingResultRecord> GetUnprocessedImportRows()
            => _appDbContext.AgentClosingResults.Where(a => a.NotificationDateTimeUtc == null);


        public async Task ExecuteNotificationRunAsync()
        {
            try
            {
                await _semaphore.WaitAsync();

                var newNotifications = GetUnprocessedImportRows();

                var tasks = newNotifications.AsEnumerable().Select(SendAgentClosingResultNotificationAsync);

                await Task.WhenAll(tasks);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task SendAgentClosingResultNotificationAsync(AgentClosingResultRecord result)
        {
            var title = $"Art Show Agent results for {result.ArtistName}";
            var message = new StringBuilder();

            message
                .AppendLine($"Dear {result.AgentName},\n\nWe'd like to inform you about the Art Show results for artist {result.ArtistName}.\n")
                .AppendLine($"Of {result.ExhibitsTotal} total exhibits:\n\n- {result.ExhibitsUnsold} remain unsold  \n- {result.ExhibitsSold} were sold (before/during closing)  \n- {result.ExhibitsToAuction} are going to the art auction  \n")
                .AppendLine($"Your expected payout as of now is {result.TotalCashAmount} € (charity percentages, where applicable, already deducted).\n\nIf any exhibits are heading to the auction, the actual payout amount may still increase.\n")
                .AppendLine($"Please pick up any unsold exhibits ({result.ExhibitsUnsold}) in the Art Show during Sales & Unsold Art Pickup times and collect your payout during Artists' Payout times.\n\nThank you!");

            var request = new SendPrivateMessageRequest()
            {
                AuthorName = "Art Show",
                Subject = title,
                Message = message.ToString(),
                RecipientRegSysId = result.AgentBadgeNo.ToString(),
                ToastTitle = "Art Show Agent Results",
                ToastMessage = $"{result.ArtistName}: ({result.ExhibitsSold} sold, {result.ExhibitsUnsold} unsold, {result.ExhibitsToAuction} to auction)"
            };

            var privateMessageId = await _privateMessageService.SendPrivateMessageAsync(request);

            result.PrivateMessageId = privateMessageId;
            result.NotificationDateTimeUtc = DateTime.UtcNow;

            _appDbContext.AgentClosingResults.Update(result);
            await _appDbContext.SaveChangesAsync();
        }

        public IList<AgentClosingNotificationResult> SimulateNotificationRun()
        {
            var newNotifications = GetUnprocessedImportRows();

            return newNotifications
                .Select(item => new AgentClosingNotificationResult()
                {
                    AgentBadgeNo = item.AgentBadgeNo,
                    AgentName = item.AgentName,
                    ArtistName = item.ArtistName,
                    ExhibitsSold = item.ExhibitsSold,
                    ExhibitsUnsold = item.ExhibitsUnsold,
                    ExhibitsToAuction = item.ExhibitsToAuction,
                    TotalCashAmount = item.TotalCashAmount
                })
                .ToList();

        }

        public async Task DeleteUnprocessedImportRowsAsync()
        {
            var recordsToDelete = GetUnprocessedImportRows();

            foreach (var record in recordsToDelete)
                _appDbContext.Remove(record);

            await _appDbContext.SaveChangesAsync();
        }
    }
}