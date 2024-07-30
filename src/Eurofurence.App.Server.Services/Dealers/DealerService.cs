using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Eurofurence.App.Common.DataDiffUtils;
using Eurofurence.App.Domain.Model.Dealers;
using Eurofurence.App.Domain.Model.Fragments;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Dealers;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using File = System.IO.File;

namespace Eurofurence.App.Server.Services.Dealers
{
    public class DealerService : EntityServiceBase<DealerRecord>,
        IDealerService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IDealerApiClient _dealerApiClient;
        private readonly ConventionSettings _conventionSettings;
        private readonly IImageService _imageService;
        private readonly ILogger _logger;

        public DealerService(
            AppDbContext appDbContext,
            IStorageServiceFactory storageServiceFactory,
            IDealerApiClient dealerApiClient,
            ConventionSettings conventionSettings,
            IImageService imageService,
            ILoggerFactory loggerFactory
        )
            : base(appDbContext, storageServiceFactory)
        {
            _appDbContext = appDbContext;
            _dealerApiClient = dealerApiClient;
            _conventionSettings = conventionSettings;
            _imageService = imageService;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public override async Task<DealerRecord> FindOneAsync(Guid id)
        {
            return await _appDbContext.Dealers
                .Include(d => d.Links)
                .AsNoTracking()
                .FirstOrDefaultAsync(entity => entity.Id == id);
        }

        public override IQueryable<DealerRecord> FindAll()
        {
            return _appDbContext.Dealers
                .Include(d => d.Links)
                .AsNoTracking();
        }

        public override async Task ReplaceOneAsync(DealerRecord entity)
        {
            var existingEntity = await _appDbContext.Dealers
                .Include(dealerRecord => dealerRecord.Links)
                .AsNoTracking()
                .FirstOrDefaultAsync(ke => ke.Id == entity.Id);

            foreach (var existingLink in existingEntity.Links)
            {
                if (!entity.Links.Contains(existingLink))
                {
                    _appDbContext.LinkFragments.Remove(existingLink);
                }
            }

            foreach (var link in entity.Links)
            {
                if (!existingEntity.Links.Contains(link))
                {
                    _appDbContext.LinkFragments.Add(link);
                }
            }
            await base.ReplaceOneAsync(entity);
        }

        public async Task RunImportAsync()
        {
            _logger.LogInformation(LogEvents.Import, "Starting dealers import.");

            if (!Directory.Exists(_conventionSettings.WorkingDirectory))
            {
                Directory.CreateDirectory(_conventionSettings.WorkingDirectory);
            }

            var csvPath = Path.Combine(_conventionSettings.WorkingDirectory, "dealers.zip");
            var newDealersExportDownloaded = await _dealerApiClient.DownloadDealersExportAsync(csvPath);

            if (!newDealersExportDownloaded)
            {
                _logger.LogError(LogEvents.Import, $"Error downloading the dealers export csv.");
                return;
            }

            var importRecords = new List<DealerRecord>();

            await using (var fileStream = File.OpenRead(csvPath))
            using (var archive = new ZipArchive(fileStream))
            {
                var csvEntry =
                    archive.Entries.Single(a => a.Name.EndsWith(".csv", StringComparison.CurrentCultureIgnoreCase));

                TextReader reader = new StreamReader(csvEntry.Open(), true);

                var csvReader = new CsvReader(reader, CultureInfo.CurrentCulture);
                csvReader.Context.RegisterClassMap<DealerImportRowClassMap>();
                csvReader.Context.Configuration.Delimiter = ";";
                var csvRecords = csvReader.GetRecords<DealerImportRow>().ToList();

                _logger.LogDebug(LogEvents.Import, $"Parsed {csvRecords.Count} records from CSV");

                for (var i = 0;  i < csvRecords.Count; i++)
                {
                    var dealerRecord = new DealerRecord
                    {
                        RegistrationNumber = csvRecords[i].RegNo,
                        AttendeeNickname = csvRecords[i].Nickname.Trim(),
                        AboutTheArtistText = csvRecords[i].AboutTheArtist.Trim(),
                        AboutTheArtText = csvRecords[i].AboutTheArt.Trim(),
                        ArtPreviewCaption = csvRecords[i].ArtPreviewCaption.Trim(),
                        DisplayName = csvRecords[i].DisplayName.Trim(),
                        ShortDescription = csvRecords[i].ShortDescription.Trim(),
                        Merchandise = csvRecords[i].Merchandise.Trim(),
                        AttendsOnThursday = !string.IsNullOrWhiteSpace(csvRecords[i].AttendsThu),
                        AttendsOnFriday = !string.IsNullOrWhiteSpace(csvRecords[i].AttendsFri),
                        AttendsOnSaturday = !string.IsNullOrWhiteSpace(csvRecords[i].AttendsSat),
                        TelegramHandle = csvRecords[i].Telegram.Trim(),
                        TwitterHandle = csvRecords[i].Twitter.Trim(),
                        DiscordHandle = csvRecords[i].Discord.Trim(),
                        MastodonHandle = csvRecords[i].Mastodon.Trim(),
                        BlueskyHandle = csvRecords[i].Bluesky.Trim(),
                        IsAfterDark = !string.IsNullOrWhiteSpace(csvRecords[i].AfterDark),
                        Keywords = csvRecords[i].GetKeywords(),
                        Categories = csvRecords[i].GetCategories()
                    };

                    dealerRecord.ArtistImageId = await GetImageIdAsync(archive, $"artist_{csvRecords[i].RegNo}.",
                        $"dealer:artist:{csvRecords[i].RegNo}");
                    dealerRecord.ArtistThumbnailImageId = await GetImageIdAsync(archive, $"thumbnail_{csvRecords[i].RegNo}.",
                        $"dealer:thumbnail:{csvRecords[i].RegNo}");
                    dealerRecord.ArtPreviewImageId =
                        await GetImageIdAsync(archive, $"art_{csvRecords[i].RegNo}.", $"dealer:art:{csvRecords[i].RegNo}");

                    ImportLinks(dealerRecord, csvRecords[i].Website);
                    SanitizeFields(dealerRecord);

                    importRecords.Add(dealerRecord);
                }
            }

            var existingRecords = FindAll();

            var patch = new PatchDefinition<DealerRecord, DealerRecord>((source, list) =>
                list.SingleOrDefault(a => a.RegistrationNumber == source.RegistrationNumber));

            patch
                .Map(s => s.RegistrationNumber, t => t.RegistrationNumber)
                .Map(s => s.AttendeeNickname, t => t.AttendeeNickname)
                .Map(s => s.AboutTheArtistText, t => t.AboutTheArtistText)
                .Map(s => s.AboutTheArtText, t => t.AboutTheArtText)
                .Map(s => s.ArtPreviewCaption, t => t.ArtPreviewCaption)
                .Map(s => s.DisplayName, t => t.DisplayName)
                .Map(s => s.ShortDescription, t => t.ShortDescription)
                .Map(s => s.Merchandise, t => t.Merchandise)
                .Map(s => s.ArtistImageId, t => t.ArtistImageId)
                .Map(s => s.ArtistThumbnailImageId, t => t.ArtistThumbnailImageId)
                .Map(s => s.ArtPreviewImageId, t => t.ArtPreviewImageId)
                .Map(s => s.TelegramHandle, t => t.TelegramHandle)
                .Map(s => s.TwitterHandle, t => t.TwitterHandle)
                .Map(s => s.DiscordHandle, t => t.DiscordHandle)
                .Map(s => s.MastodonHandle, t => t.MastodonHandle)
                .Map(s => s.BlueskyHandle, t => t.BlueskyHandle)
                .Map(s => s.AttendsOnThursday, t => t.AttendsOnThursday)
                .Map(s => s.AttendsOnFriday, t => t.AttendsOnFriday)
                .Map(s => s.AttendsOnSaturday, t => t.AttendsOnSaturday)
                .Map(s => s.Categories, t => t.Categories)
                .Map(s => s.IsAfterDark, t => t.IsAfterDark)
                .Map(s => s.Links, t => t.Links)
                .Map(s => s.Keywords, t => t.Keywords);

            var diff = patch.Patch(importRecords, existingRecords);
            _appDbContext.ChangeTracker.Clear();
            await ApplyPatchOperationAsync(diff);

            _logger.LogDebug(LogEvents.Import, $"Added: {diff.Count(a => a.Action == ActionEnum.Add)}");
            _logger.LogDebug(LogEvents.Import, $"Deleted: {diff.Count(a => a.Action == ActionEnum.Delete)}");
            _logger.LogDebug(LogEvents.Import, $"Updated: {diff.Count(a => a.Action == ActionEnum.Update)}");
            _logger.LogDebug(LogEvents.Import, $"Not Modified: {diff.Count(a => a.Action == ActionEnum.NotModified)}");

            File.Delete(csvPath);
            _logger.LogInformation(LogEvents.Import, "Dealers import finished successfully.");
        }

        private void SanitizeFields(DealerRecord dealerRecord)
        {
            dealerRecord.TwitterHandle =
                dealerRecord.TwitterHandle
                    .Replace("@", "")
                    .Replace("http://twitter.com/", "")
                    .Replace("https://twitter.com/", "");

            dealerRecord.TelegramHandle =
                dealerRecord.TelegramHandle
                    .Replace("@", "")
                    .Replace("https://t.me/", "")
                    .Replace("https://telegram.me/", "");

            dealerRecord.DiscordHandle =
                dealerRecord.DiscordHandle
                    .Replace("@", "")
                    .Replace("http://discord.com/users/", "")
                    .Replace("https://discord.com/users/", "")
                    .Replace("http://discordapp.com/users/", "")
                    .Replace("https://discordapp.com/users/", "");

            dealerRecord.BlueskyHandle =
                dealerRecord.BlueskyHandle
                    .Replace("@", "")
                    .Replace("http://bsky.app/profile/", "")
                    .Replace("https://bsky.app/profile/", "");

            dealerRecord.ShortDescription = ConvertKnownUnicodeCharacters(dealerRecord.ShortDescription);
            dealerRecord.AboutTheArtistText = ConvertKnownUnicodeCharacters(dealerRecord.AboutTheArtistText);
            dealerRecord.AboutTheArtText = ConvertKnownUnicodeCharacters(dealerRecord.AboutTheArtText);
            dealerRecord.ArtPreviewCaption = ConvertKnownUnicodeCharacters(dealerRecord.ArtPreviewCaption);
        }

        private string ConvertKnownUnicodeCharacters(string input)
        {
            return input.Replace("\u2028", "\n");
        }

        private void ImportLinks(DealerRecord dealerRecord, string websiteUrls)
        {
            if (string.IsNullOrWhiteSpace(websiteUrls)) return;

            var linkFragments = new List<LinkFragment>();

            var sanitizedParts = websiteUrls
                .Replace(" / ", ";")
                .Split(new[] { ' ', ',', ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in sanitizedParts)
            {
                var assumedUri = part;
                if (part.Length < 10) continue;

                if (!assumedUri.StartsWith("http://", StringComparison.CurrentCultureIgnoreCase) &&
                    !assumedUri.StartsWith("https://", StringComparison.CurrentCultureIgnoreCase))
                    assumedUri = $"http://{part}";

                if (Uri.IsWellFormedUriString(assumedUri, UriKind.Absolute)) { }
                linkFragments.Add(new LinkFragment()
                {
                    FragmentType = LinkFragment.FragmentTypeEnum.WebExternal,
                    Name = string.Empty,
                    Target = assumedUri
                });
            }

            dealerRecord.Links = linkFragments;
        }

        private async Task<Guid?> GetImageIdAsync(ZipArchive archive, string fileNameStartsWith,
            string internalReference)
        {
            var imageEntry =
                archive.Entries.SingleOrDefault(
                    a => a.Name.StartsWith(fileNameStartsWith, StringComparison.CurrentCultureIgnoreCase));

            if (imageEntry == null) return null;

            var existingImage = await _imageService.FindAll()
                .FirstOrDefaultAsync(image => image.InternalReference == internalReference);

            var stream = imageEntry.Open();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            if (existingImage != null)
            {
                if (imageEntry.Length != existingImage.SizeInBytes)
                {
                    await _imageService.ReplaceImageAsync(existingImage.Id, internalReference, ms);
                }
                return existingImage.Id;
            }
            else
            {
                var result = await _imageService.InsertImageAsync(internalReference, ms);
                return result?.Id;
            }
        }
    }

    public sealed class DealerImportRowClassMap : ClassMap<DealerImportRow>
    {
        public DealerImportRowClassMap()
        {
            Map(m => m.RegNo).Name("Reg No.");
            Map(m => m.Nickname).Name("Nick");
            Map(m => m.DisplayName).Name("Display Name");
            Map(m => m.Merchandise).Name("Merchandise");
            Map(m => m.ShortDescription).Name("Short Description");
            Map(m => m.AboutTheArtist).Name("About the Artist");
            Map(m => m.AboutTheArt).Name("About the Art");
            Map(m => m.ArtPreviewCaption).Name("Art Preview Caption");
            Map(m => m.Website).Name("Website");
            Map(m => m.Telegram).Name("Telegram");
            Map(m => m.Twitter).Name("Twitter");
            Map(m => m.Discord).Name("Discord");
            Map(m => m.Mastodon).Name("Mastodon");
            Map(m => m.Bluesky).Name("Bluesky");
            Map(m => m.AttendsThu).Name("Attends Thu");
            Map(m => m.AttendsFri).Name("Attends Fri");
            Map(m => m.AttendsSat).Name("Attends Sat");
            Map(m => m.AfterDark).Name("After Dark");
            Map(m => m.Keywords).Name("Keywords");
        }
    }

    public class DealerImportRow
    {
        public int RegNo { get; set; }
        public string Nickname { get; set; }
        public string DisplayName { get; set; }
        public string Merchandise { get; set; }
        public string ShortDescription { get; set; }
        public string AboutTheArtist { get; set; }
        public string AboutTheArt { get; set; }
        public string ArtPreviewCaption { get; set; }
        public string AttendsThu { get; set; }
        public string AttendsFri { get; set; }
        public string AttendsSat { get; set; }
        public string Website { get; set; }
        public string Telegram { get; set; }
        public string Twitter { get; set; }
        public string Discord { get; set; }
        public string Mastodon { get; set; }
        public string Bluesky { get; set; }
        public string AfterDark { get; set; }
        public string Keywords { get; set; }

        public string[] GetCategories()
        {
            if (string.IsNullOrEmpty(Keywords))
            {
                return [];
            }

            var keywords = GetKeywords();

            return [.. keywords.Keys];
        }

        public Dictionary<string, string[]> GetKeywords()
        {
            if (string.IsNullOrEmpty(Keywords))
            {
                return new Dictionary<string, string[]>();
            }

            return JsonSerializer.Deserialize<Dictionary<string, string[]>>(Keywords);
        }
    }
}