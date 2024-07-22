using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Eurofurence.App.Common.DataDiffUtils;
using Eurofurence.App.Common.ExtensionMethods;
using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Announcements;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Eurofurence.App.Server.Web.Jobs
{
    public class UpdateAnnouncementsJob : IJob
    {
        private readonly IAnnouncementService _announcementService;
        private readonly IImageService _imageService;
        private readonly IPushEventMediator _pushEventMediator;
        private readonly AnnouncementConfiguration _configuration;
        private readonly ILogger _logger;

        public UpdateAnnouncementsJob(
            IAnnouncementService announcementService, 
            IImageService imageService,
            IPushEventMediator pushEventMediator,
            AnnouncementConfiguration configuration,
            ILoggerFactory loggerFactory)
        {
            _announcementService = announcementService;
            _imageService = imageService;
            _pushEventMediator = pushEventMediator;
            _configuration = configuration;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation(LogEvents.Import, $"Starting job {context.JobDetail.Key.Name}");

            try
            {
                var response = string.Empty;
                using (var client = new HttpClient())
                {
                    var url = _configuration.Url;
                    if (String.IsNullOrWhiteSpace(url))
                    {
                        _logger.LogDebug(LogEvents.Import, "Empty soruce url; cancelling job", url);
                        return;
                    }

                    _logger.LogDebug(LogEvents.Import, "Fetching data from {url}", url);
                    response = await client.GetStringAsync(url);
                }

                if (response == "null")
                {
                    _logger.LogDebug(LogEvents.Import, "Received null response");
                    return;
                }

                var jsonDocument = JsonDocument.Parse(response);
                var records = jsonDocument.RootElement.EnumerateArray();

                var unixReference = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

                var mapping = records.Select(j => new
                {
                    Record = new AnnouncementRecord()
                    {
                        ExternalReference = j.GetProperty("id").GetString(),
                        Area = j.GetProperty("news").GetProperty("type").GetString().UppercaseFirst(),
                        Author = j.GetProperty("news").GetProperty("department").GetString().UppercaseFirst() ?? "Eurofurence",
                        Title = j.GetProperty("news").GetProperty("title").GetString(),
                        Content = j.GetProperty("news").GetProperty("message").GetString(),
                        ValidFromDateTimeUtc = unixReference.AddSeconds(j.GetProperty("date").GetDouble()).ToUniversalTime(),
                        ValidUntilDateTimeUtc = unixReference
                            .AddSeconds(j.GetProperty("news").GetProperty("valid_until").GetDouble()).ToUniversalTime(),
                        ImageId = GetImageIdForEntryAsync(j.GetProperty("id").GetString(), j.GetProperty("data").GetProperty("imagedata").GetString()).Result
                    },
                    Type = j.GetProperty("news").GetProperty("type").GetString()
                }).ToList();

                foreach (var item in mapping)
                    if (new[] { "new", "reschedule" }.Contains(item.Type))
                        item.Record.ValidUntilDateTimeUtc = item.Record.ValidFromDateTimeUtc.AddHours(48);

                var existingRecords = _announcementService.FindAll();

                var patch = new PatchDefinition<AnnouncementRecord, AnnouncementRecord>((source, list) =>
                    list.SingleOrDefault(a => a.ExternalReference == source.ExternalReference));

                patch
                    .Map(s => s.ExternalReference, t => t.ExternalReference)
                    .Map(s => s.Area, t => t.Area)
                    .Map(s => s.Author, t => t.Author)
                    .Map(s => s.Title, t => t.Title)
                    .Map(s => s.Content, t => t.Content)
                    .Map(s => s.ImageId, t => t.ImageId)
                    .Map(s => s.ValidUntilDateTimeUtc, t => t.ValidUntilDateTimeUtc)
                    .Map(s => s.ValidFromDateTimeUtc, t => t.ValidFromDateTimeUtc);

                var diff = patch.Patch(mapping.Select(a => a.Record), existingRecords)
                    .Where(a => !string.IsNullOrEmpty(a.Entity.ExternalReference) && a.Action != ActionEnum.NotModified)
                    .ToList();

                _logger.LogDebug(LogEvents.Import, "Diff results in {count} new/modified records", diff.Count);

                if (diff.Count == 0) return;

                _logger.LogInformation(LogEvents.Import, "Processing {count} new/modified records", diff.Count);

                await _announcementService.ApplyPatchOperationAsync(diff);
                await _pushEventMediator.PushSyncRequestAsync();

                foreach (var record in diff.Where(a => a.Action == ActionEnum.Add))
                {
                    _logger.LogInformation(LogEvents.Import, "Sending push notification for announcement {id} ({title})", record.Entity.Id, record.Entity.Title);
                    await _pushEventMediator.PushAnnouncementNotificationAsync(record.Entity);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(LogEvents.Import, $"Job {context.JobDetail.Key.Name} failed with exception {e.Message} {e.StackTrace}");
            }

        }

        private async Task<Guid?> GetImageIdForEntryAsync(string reference, string imageDataBase64)
        {
            reference = $"announcements:{reference}";

            if (string.IsNullOrWhiteSpace(imageDataBase64)) return null;
            var imageBytes = Convert.FromBase64String(imageDataBase64);

            using MemoryStream ms = new(imageBytes);
            var image = await _imageService.InsertImageAsync(reference, ms);
            return image.Id;
        }
    }
}