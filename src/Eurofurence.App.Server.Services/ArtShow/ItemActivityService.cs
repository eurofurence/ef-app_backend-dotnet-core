using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.ArtShow;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.ArtShow;
using Eurofurence.App.Server.Services.Abstractions.Communication;

namespace Eurofurence.App.Server.Services.ArtShow
{
    public class ItemActivityService :IItemActivityService
    {
        private readonly ConventionSettings _conventionSettings;
        private readonly IEntityRepository<ItemActivityRecord> _itemActivityRepository;
        private readonly IPrivateMessageService _privateMessageService;

        public ItemActivityService(
            ConventionSettings conventionSettings, 
            IEntityRepository<ItemActivityRecord> itemActivityRepository,
            IPrivateMessageService privateMessageService
            )
        {
            _conventionSettings = conventionSettings;
            _itemActivityRepository = itemActivityRepository;
            _privateMessageService = privateMessageService;
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


        private class NotificationBundle
        {
            public string RecipientUid { get; set; }
            public IList<ItemActivityRecord> ItemsSold { get; set; }
            public IList<ItemActivityRecord> ItemsToAuction { get; set; }
        }

        private async Task<IList<NotificationBundle>> BuildNotificationBundlesAsync()
        {
            var newActivities = await _itemActivityRepository.FindAllAsync(a => a.NotificationDateTimeUtc == null);

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
            var notificationBundles = await BuildNotificationBundlesAsync();

            var tasks = notificationBundles.Select(async bundle =>
            {
                if (bundle.ItemsSold?.Count > 0) await SendItemsSoldNotificationAsync(bundle.RecipientUid, bundle.ItemsSold);
                if (bundle.ItemsToAuction?.Count > 0) await SendItemsToAuctionNotificationAsync(bundle.RecipientUid, bundle.ItemsToAuction);
            });

            await Task.WhenAll(tasks);
        }

        private async Task SendItemsToAuctionNotificationAsync(string recipientUid, IList<ItemActivityRecord> items)
        {
            var title = $"{items.Count} item(s) will participate in the auction";
            var message = new StringBuilder();

            message.AppendLine(
                $"{items.Count} item(s) on which you have been the last bidder will be part of the auction.\n");

            foreach (var item in items)
                message.AppendLine($"{item.ASIDNO}: \"{item.ArtPieceTitle}\" by \"{item.ArtistName}\"");

            message.AppendLine("\nIf you wish to defend your current bids against other potential higher bids, please attend the auction.\n\nThank you!");

            var request = new SendPrivateMessageRequest()
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
                await _itemActivityRepository.ReplaceOneAsync(item);
            }
        }

        private async Task SendItemsSoldNotificationAsync(string recipientUid, IList<ItemActivityRecord> items)
        {
            var title = $"You have won {items.Count} item(s) from the Art Show";
            var message = new StringBuilder();

            message.AppendLine(
                $"Congratulations! You have won {items.Count} item(s) from the Art Show:\n");

            foreach (var item in items)
                message.AppendLine($"{item.ASIDNO}: \"{item.ArtPieceTitle}\" by \"{item.ArtistName}\"");

            message.AppendLine("\nPlease pick them up at the Art Show during sales hours (these are announced in the event schedule and can be found both in your con book or the mobile app).\n\nThank you!");

            var request = new SendPrivateMessageRequest()
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
                await _itemActivityRepository.ReplaceOneAsync(item);
            }
        }

        public async Task<IList<NotificationResult>> SimulateNotificationRunAsync()
        {
            var notificationBundles = await BuildNotificationBundlesAsync();

            return notificationBundles
                .Select(bundle => new NotificationResult()
                {
                    RecipientUid = bundle.RecipientUid,
                    IdsSold = bundle.ItemsSold.Select(a => a.ASIDNO).ToList(),
                    IdsToAuction = bundle.ItemsToAuction.Select(a => a.ASIDNO).ToList()
                })
                .ToList();
        }
    }
}
