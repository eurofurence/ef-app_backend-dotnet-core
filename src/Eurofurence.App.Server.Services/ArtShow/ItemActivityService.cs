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
    public class ItemActivityService : IItemActivityService
    {
        private readonly AppDbContext _appDbContext;
        private readonly ConventionSettings _conventionSettings;
        private readonly IPrivateMessageService _privateMessageService;
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public ItemActivityService(
            AppDbContext appDbContext,
            ConventionSettings conventionSettings,
            IPrivateMessageService privateMessageService
            )
        {
            _appDbContext = appDbContext;
            _conventionSettings = conventionSettings;
            _privateMessageService = privateMessageService;
        }

        public async Task<ImportResult> ImportActivityLogAsync(TextReader logReader)
        {
            var importResult = new ImportResult();
            var csv = new CsvReader(logReader, CultureInfo.InvariantCulture);

            csv.Context.RegisterClassMap<LogImportRowClassMap>();
            csv.Context.Configuration.Delimiter = ",";

            await foreach (var csvRecord in csv.GetRecordsAsync<LogImportRow>())
            {
                var existingRecord = await _appDbContext.ItemActivitys.AsNoTracking().FirstOrDefaultAsync(a => a.ImportHash == csvRecord.Hash.Value);
                if (existingRecord != null)
                {
                    importResult.RowsSkippedAsDuplicate++;
                    continue;
                }

                var newRecord = new ItemActivityRecord()
                {
                    Id = Guid.NewGuid(),
                    OwnerUid = csvRecord.RegNo.ToString(),
                    ASIDNO = csvRecord.ASIDNO,
                    ArtistName = csvRecord.ArtistName,
                    ArtPieceTitle = csvRecord.ArtPieceTitle,
                    Status = (ItemActivityRecord.StatusEnum)Enum.Parse(typeof(ItemActivityRecord.StatusEnum),
                        csvRecord.Status, true),
                    FinalBidAmount = csvRecord.FinalBidAmount,
                    ImportDateTimeUtc = DateTime.UtcNow,
                    ImportHash = csvRecord.Hash.Value
                };
                newRecord.Touch();

                _appDbContext.ItemActivitys.Add(newRecord);
                importResult.RowsImported++;
            }

            await _appDbContext.SaveChangesAsync();
            return importResult;
        }

        private IQueryable<ItemActivityRecord> GetUnprocessedImportRows()
            => _appDbContext.ItemActivitys.Where(a => a.NotificationDateTimeUtc == null);

        private class NotificationBundle
        {
            public string RecipientUid { get; set; }
            public IList<ItemActivityRecord> ItemsSold { get; set; }
            public IList<ItemActivityRecord> ItemsToAuction { get; set; }
        }

        private IList<NotificationBundle> BuildNotificationBundles()
        {
            var newActivities = GetUnprocessedImportRows();

            return newActivities
                .GroupBy(a => a.OwnerUid)
                .Select(a => new NotificationBundle()
                {
                    RecipientUid = a.Key,
                    ItemsSold = a.Where(b => b.Status == ItemActivityRecord.StatusEnum.Sold).ToList(),
                    ItemsToAuction = a.Where(b => b.Status == ItemActivityRecord.StatusEnum.Auction).ToList()
                })
                .ToList();
        }

        public async Task ExecuteNotificationRunAsync()
        {
            try
            {
                await _semaphore.WaitAsync();

                var notificationBundles = BuildNotificationBundles();

                foreach (var bundle in notificationBundles)
                {
                    if (bundle.ItemsSold?.Count > 0) await SendItemsSoldNotificationAsync(bundle.RecipientUid, bundle.ItemsSold);
                    if (bundle.ItemsToAuction?.Count > 0) await SendItemsToAuctionNotificationAsync(bundle.RecipientUid, bundle.ItemsToAuction);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task SendItemsToAuctionNotificationAsync(string recipientUid, IList<ItemActivityRecord> items)
        {
            var title = $"{items.Count} item(s) will participate in the auction";
            var message = new StringBuilder();

            message.AppendLine(
                $"{items.Count} item(s) on which you have been the last bidder will be part of the auction.\n");

            foreach (var item in items)
            {
                if (item.FinalBidAmount is { } finalBidAmount && finalBidAmount > 0)
                {
                    message.AppendLine($"- {item.ASIDNO}: \"{item.ArtPieceTitle}\" by \"{item.ArtistName}\" with final bid before auction {item.FinalBidAmount} €");
                }
                else {
                    message.AppendLine($"- {item.ASIDNO}: \"{item.ArtPieceTitle}\" by \"{item.ArtistName}\"");
                }
            }

            message.AppendLine("\nIf you wish to defend your current bids against other potential higher bids, please attend the auction.\n\nThank you!\n\n(Disclaimer: final bid amounts not guaranteed to be accurate, please double check with the posted listings or contact the Art Show team in case of irregularities.)");

            var request = new SendPrivateMessageByRegSysRequest()
            {
                AuthorName = "Art Show",
                Subject = title,
                Message = message.ToString(),
                RecipientUid = recipientUid,
                ToastTitle = "Art Show Results",
                ToastMessage = title
            };

            var privateMessageId = await _privateMessageService.SendPrivateMessageAsync(request);
            var now = DateTime.UtcNow;

            foreach (var item in items)
            {
                item.PrivateMessageId = privateMessageId;
                item.NotificationDateTimeUtc = now;
                _appDbContext.Update(item);
            }

            await _appDbContext.SaveChangesAsync();
        }

        private async Task SendItemsSoldNotificationAsync(string recipientUid, IList<ItemActivityRecord> items)
        {
            var title = $"You have won {items.Count} item(s) from the Art Show";
            var message = new StringBuilder();

            message.AppendLine(
                $"Congratulations! You have won {items.Count} item(s) from the Art Show:\n");

            var totalDues = 0;
            foreach (var item in items)
            {
                if (item.FinalBidAmount is { } finalBidAmount && finalBidAmount > 0)
                {
                    message.AppendLine($"- {item.ASIDNO}: \"{item.ArtPieceTitle}\" by \"{item.ArtistName}\" for a final bid of {finalBidAmount} €");
                    totalDues += finalBidAmount;
                }
                else
                {
                    message.AppendLine($"- {item.ASIDNO}: \"{item.ArtPieceTitle}\" by \"{item.ArtistName}\"");
                }
            }

            if (totalDues > 0)
                message.AppendLine($"\nYour expected grand total is: {totalDues} €");

            message.AppendLine("\nPlease make sure you have sufficient cash with you and pay your items at the Security Desk in the foyer before proceeding to pick them up at the Art Show during Sales & Pickup. The times for Sales & Pickup are announced in the event schedule and can be found in your conbook, the schedule on our website or right here in the mobile app.\n\nThank you!\n\n(Disclaimer: expected grand total only includes items with final bid in listing above, won items and final bid amounts are purely informative and not guaranteed to be accurate, please double check with the posted listings or contact the Art Show team in case of irregularities.)");

            var request = new SendPrivateMessageByRegSysRequest()
            {
                AuthorName = "Art Show",
                Subject = title,
                Message = message.ToString(),
                RecipientUid = recipientUid,
                ToastTitle = "Art Show Results",
                ToastMessage = title
            };

            var privateMessageId = await _privateMessageService.SendPrivateMessageAsync(request);
            var now = DateTime.UtcNow;

            foreach (var item in items)
            {
                item.PrivateMessageId = privateMessageId;
                item.NotificationDateTimeUtc = now;
                _appDbContext.Update(item);
            }

            await _appDbContext.SaveChangesAsync();
        }

        public IList<ItemActivityNotificationResult> SimulateNotificationRun()
        {
            var notificationBundles = BuildNotificationBundles();

            return notificationBundles
                .Select(bundle => new ItemActivityNotificationResult()
                {
                    RecipientUid = bundle.RecipientUid,
                    IdsSold = bundle.ItemsSold.Select(a => a.ASIDNO).ToList(),
                    IdsToAuction = bundle.ItemsToAuction.Select(a => a.ASIDNO).ToList(),
                    GrandTotal = bundle.ItemsSold.Sum(i => (i.FinalBidAmount is { } finalBidAmount && finalBidAmount > 0) ? finalBidAmount : 0),
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
