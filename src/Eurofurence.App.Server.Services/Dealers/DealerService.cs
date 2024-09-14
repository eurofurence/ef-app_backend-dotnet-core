using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using Eurofurence.App.Common.DataDiffUtils;
using Eurofurence.App.Domain.Model.Dealers;
using Eurofurence.App.Domain.Model.Fragments;
using Eurofurence.App.Domain.Model.Sync;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Dealers;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Server.Services.Abstractions.Sanitization;
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
        private readonly IHttpUriSanitizer _uriSanitizer;
        private readonly ILogger _logger;
        private static SemaphoreSlim _semaphore = new(1, 1);

        public DealerService(
            AppDbContext appDbContext,
            IStorageServiceFactory storageServiceFactory,
            IDealerApiClient dealerApiClient,
            ConventionSettings conventionSettings,
            IImageService imageService,
            IHttpUriSanitizer uriSanitizer,
            ILoggerFactory loggerFactory
        )
            : base(appDbContext, storageServiceFactory)
        {
            _appDbContext = appDbContext;
            _dealerApiClient = dealerApiClient;
            _conventionSettings = conventionSettings;
            _imageService = imageService;
            _uriSanitizer = uriSanitizer;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public override async Task<DealerRecord> FindOneAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Dealers
                .Include(d => d.Links)
                .AsNoTracking()
                .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        }

        public override IQueryable<DealerRecord> FindAll()
        {
            return _appDbContext.Dealers
                .Include(d => d.Links)
                .AsNoTracking();
        }

        public override async Task ReplaceOneAsync(DealerRecord entity, CancellationToken cancellationToken = default)
        {
            var existingEntity = await _appDbContext.Dealers
                .Include(dealerRecord => dealerRecord.Links)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == entity.Id, cancellationToken);

            foreach (var existingLink in existingEntity.Links)
            {
                var entityLinkInNewEntity = entity.Links.FirstOrDefault(link => Equals(link, existingLink));

                if (entityLinkInNewEntity != null)
                {
                    entityLinkInNewEntity.Id = existingLink.Id;
                }
                else
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

            await base.ReplaceOneAsync(entity, cancellationToken);
        }

        public override async Task DeleteOneAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var existingEntity = await _appDbContext.Dealers
                .Include(d => d.Links)
                .Include(d => d.ArtistImage)
                .Include(d => d.ArtistThumbnailImage)
                .Include(d => d.ArtPreviewImage)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

            if (existingEntity.Links.Count > 0)
            {
                _appDbContext.LinkFragments.RemoveRange(existingEntity.Links);
                await _appDbContext.SaveChangesAsync(cancellationToken);
            }
            if (existingEntity.ArtistImageId is { } artistImageId) await _imageService.DeleteOneAsync(artistImageId, cancellationToken);
            if (existingEntity.ArtistThumbnailImageId is { } artistThumbnailImageId) await _imageService.DeleteOneAsync(artistThumbnailImageId, cancellationToken);
            if (existingEntity.ArtPreviewImageId is { } artPreviewImageId) await _imageService.DeleteOneAsync(artPreviewImageId, cancellationToken);

            await base.DeleteOneAsync(id, cancellationToken);
        }

        public override async Task<DeltaResponse<DealerRecord>> GetDeltaResponseAsync(
            DateTime? minLastDateTimeChangedUtc = null,
            CancellationToken cancellationToken = default)
        {
            var storageInfo = await GetStorageInfoAsync(cancellationToken);
            var response = new DeltaResponse<DealerRecord>
            {
                StorageDeltaStartChangeDateTimeUtc = storageInfo.DeltaStartDateTimeUtc,
                StorageLastChangeDateTimeUtc = storageInfo.LastChangeDateTimeUtc
            };

            if (!minLastDateTimeChangedUtc.HasValue || minLastDateTimeChangedUtc < storageInfo.DeltaStartDateTimeUtc)
            {
                response.RemoveAllBeforeInsert = true;
                response.DeletedEntities = Array.Empty<Guid>();
                response.ChangedEntities = await
                    _appDbContext.Dealers
                        .Include(d => d.Links)
                        .Where(entity => entity.IsDeleted == 0)
                        .ToArrayAsync(cancellationToken);
            }
            else
            {
                response.RemoveAllBeforeInsert = false;

                var entities = _appDbContext.Dealers
                    .Include(d => d.Links)
                    .IgnoreQueryFilters()
                    .Where(entity => entity.LastChangeDateTimeUtc > minLastDateTimeChangedUtc);

                response.ChangedEntities = await entities
                    .Where(a => a.IsDeleted == 0)
                    .ToArrayAsync(cancellationToken);
                response.DeletedEntities = await entities
                    .Where(a => a.IsDeleted == 1)
                    .Select(a => a.Id)
                    .ToArrayAsync(cancellationToken);
            }

            return response;
        }

        public async Task RunImportAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _semaphore.WaitAsync();
                _logger.LogDebug(LogEvents.Import, "Starting dealers import.");

                if (!Directory.Exists(_conventionSettings.WorkingDirectory))
                {
                    Directory.CreateDirectory(_conventionSettings.WorkingDirectory);
                }

                var dealerPackagePath = Path.Combine(_conventionSettings.WorkingDirectory, "dealers.zip");
                var newDealersExportDownloaded = await _dealerApiClient.DownloadDealersExportAsync(dealerPackagePath);

                if (!newDealersExportDownloaded)
                {
                    _logger.LogError(LogEvents.Import, $"Error downloading the dealers export csv.");
                    return;
                }

                var importRecords = new List<DealerRecord>();

                await using (var fileStream = File.OpenRead(dealerPackagePath))
                using (var archive = new ZipArchive(fileStream))
                {
                    var csvEntry =
                        archive.Entries.Single(a => a.Name.EndsWith(".csv", StringComparison.InvariantCultureIgnoreCase));

                    TextReader reader = new StreamReader(csvEntry.Open(), true);

                    var badData = new List<string>();

                    var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        Delimiter = ";",
                        HasHeaderRecord = true,
                        TrimOptions = TrimOptions.Trim,
                        NewLine = "\n",
                        BadDataFound = arg => badData.Add(arg.Context.Parser.RawRecord)
                    };

                    var csvReader = new CsvReader(reader, csvConfiguration);
                    csvReader.Context.RegisterClassMap<DealerImportRowClassMap>();
                    var csvRecords = csvReader.GetRecords<DealerImportRow>().ToList();

                    _logger.LogDebug(LogEvents.Import, $"Parsed {csvRecords.Count} records from CSV");

                    for (var i = 0; i < csvRecords.Count; i++)
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

                        dealerRecord.ArtistImageId = await GetImageIdAsync(
                            archive,
                            $"artist_{csvRecords[i].RegNo}.",
                            $"dealer:artist:{csvRecords[i].RegNo}",
                            cancellationToken
                        );
                        dealerRecord.ArtistThumbnailImageId = await GetImageIdAsync(
                            archive,
                            $"thumbnail_{csvRecords[i].RegNo}.",
                            $"dealer:thumbnail:{csvRecords[i].RegNo}",
                            cancellationToken
                        );
                        dealerRecord.ArtPreviewImageId = await GetImageIdAsync(archive,
                            $"art_{csvRecords[i].RegNo}.",
                            $"dealer:art:{csvRecords[i].RegNo}",
                            cancellationToken
                        );

                        ImportLinks(dealerRecord, csvRecords[i].Website);
                        SanitizeFields(dealerRecord);

                        importRecords.Add(dealerRecord);
                    }

                    if (badData.Count > 0)
                    {
                        _logger.LogInformation($"Found {badData.Count} bad rows:\n{string.Join("\n", badData)}");
                    }
                }

                var existingRecords = FindAll();

                var patch = new PatchDefinition<DealerRecord, DealerRecord>((source, list) =>
                    list.SingleOrDefault(d => d.RegistrationNumber == source.RegistrationNumber));

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
                await ApplyPatchOperationAsync(diff, cancellationToken);

                File.Delete(dealerPackagePath);
                _logger.LogInformation(LogEvents.Import,
                    $"Dealers import with {diff.Count(p => p.Action == ActionEnum.Add)} addition(s), {diff.Count(p => p.Action == ActionEnum.Update)} update(s) and {diff.Count(p => p.Action == ActionEnum.Delete)} deletion(s) finished successfully with {diff.Count(a => a.Action == ActionEnum.NotModified)} unmodified.");
            }
            finally
            {
                _semaphore.Release();
            }
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

                if (_uriSanitizer.Sanitize(assumedUri) is string sanitizedUrl and not null)
                    assumedUri = sanitizedUrl;
                else
                    continue;

                linkFragments.Add(new LinkFragment()
                {
                    FragmentType = LinkFragment.FragmentTypeEnum.WebExternal,
                    Name = string.Empty,
                    Target = assumedUri
                });
            }

            dealerRecord.Links = linkFragments;
        }

        private async Task<Guid?> GetImageIdAsync(
            ZipArchive archive,
            string fileNameStartsWith,
            string internalReference,
            CancellationToken cancellationToken)
        {
            var imageEntry =
                archive.Entries.SingleOrDefault(
                    a => a.Name.StartsWith(fileNameStartsWith, StringComparison.InvariantCultureIgnoreCase));

            if (imageEntry == null) return null;

            var existingImage = await _imageService.FindAll()
                .FirstOrDefaultAsync(image => image.InternalReference == internalReference, cancellationToken);

            var stream = imageEntry.Open();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms, cancellationToken);
            if (existingImage != null)
            {
                var updatedImageRecord = await _imageService.ReplaceImageAsync(existingImage.Id, internalReference, ms, cancellationToken: cancellationToken);
                return updatedImageRecord.Id;
            }

            var result = await _imageService.InsertImageAsync(internalReference, ms, cancellationToken: cancellationToken);
            return result?.Id;
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