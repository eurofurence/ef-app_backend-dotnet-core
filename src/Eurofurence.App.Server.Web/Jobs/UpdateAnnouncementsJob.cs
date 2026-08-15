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
using Eurofurence.App.Server.Services.Abstractions.Events;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Sentry;

namespace Eurofurence.App.Server.Web.Jobs
{
    [DisallowConcurrentExecution]
    public class UpdateAnnouncementsJob : IJob
    {
        private readonly IAnnouncementService _announcementService;
        private readonly IImageService _imageService;
        private readonly IPushNotificationChannelManager _pushNotificationChannelManager;
        private readonly IEventService _eventService;
        private readonly AnnouncementOptions _announcementOptions;
        private readonly ILogger _logger;

        public UpdateAnnouncementsJob(
            IAnnouncementService announcementService,
            IImageService imageService,
            IPushNotificationChannelManager pushNotificationChannelManager,
            IEventService eventService,
            IOptions<AnnouncementOptions> announcementOptions,
            ILoggerFactory loggerFactory)
        {
            _announcementService = announcementService;
            _imageService = imageService;
            _pushNotificationChannelManager = pushNotificationChannelManager;
            _eventService = eventService;
            _announcementOptions = announcementOptions.Value;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation(LogEvents.Import, "Starting job {Name}", context.JobDetail.Key.Name);

            try
            {
                string response;

                try
                {
                    using (var client = new HttpClient())
                    {
                        var url = _announcementOptions.Url;
                        if (string.IsNullOrWhiteSpace(url))
                        {
                            _logger.LogError(LogEvents.Import, "Empty source url; cancelling job");
                            return;
                        }

                        _logger.LogDebug(LogEvents.Import, "Fetching data from {url}", url);
                        response = await client.GetStringAsync(url);
                    }
                }
                catch (Exception ex)
                {
                    SentrySdk.CaptureException(ex);
                    _logger.LogError(LogEvents.Import, ex, "Failed to retrieve data from announcements API");
                    return;
                }

                if (response == "null")
                {
                    SentrySdk.CaptureMessage("Received 'null' response from announcements API.", SentryLevel.Debug);
                    _logger.LogDebug(LogEvents.Import, "Received null response");
                    return;
                }

                var jsonDocument = JsonDocument.Parse(response);
                var records = jsonDocument.RootElement.EnumerateArray();

                var unixReference = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

                var mapping = records.Select(j =>
                {
                    var validFromDateTimeUtc = unixReference.AddSeconds(double.Parse(j.GetProperty("date").ToString())).ToUniversalTime();
                    return new
                    {
                        Record = new AnnouncementRecord()
                        {
                            ExternalReference = j.GetProperty("id").GetString(),
                            Area = j.GetProperty("news").GetProperty("type").GetString().UppercaseFirst(),
                            Author = j.GetProperty("news").GetProperty("department").GetString().UppercaseFirst() ??
                                 "Eurofurence",
                            Title = j.GetProperty("news").GetProperty("title").GetString(),
                            Content = j.GetProperty("news").GetProperty("message").GetString(),
                            ValidFromDateTimeUtc = validFromDateTimeUtc,
                            ValidUntilDateTimeUtc = validFromDateTimeUtc.AddHours(_announcementOptions.AnnouncementValidityHours),
                            ImageId = j.GetProperty("data").TryGetProperty("imagedata", out var _) == true ? GetImageIdForEntryAsync(j.GetProperty("id").GetString(),
                            j.GetProperty("data").GetProperty("imagedata").GetString()).Result : null
                        },
                        Type = j.GetProperty("news").GetProperty("type").GetString()
                    };
                }).ToList();

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
                    .Where(a =>
                        // Only events from this job should have an ExternalReference.
                        !string.IsNullOrEmpty(a.Entity.ExternalReference) &&
                        // Anything that has not been modified can be ignored.
                        a.Action != ActionEnum.NotModified &&
                        (
                            // Anything that is not a deletion must be kept…
                            a.Action != ActionEnum.Delete ||
                            // … unless it is an expired announcement, which must be deleted.
                            a.Entity.ValidUntilDateTimeUtc.CompareTo(DateTime.UtcNow) < 0
                        )
                    )
                    .ToList();

                _logger.LogDebug(LogEvents.Import, "Diff results in {count} new/modified records", diff.Count);

                if (diff.Count == 0) return;

                _logger.LogInformation(LogEvents.Import, "Processing {count} new/modified records", diff.Count);

                await _announcementService.ApplyPatchOperationAsync(diff);

                await _pushNotificationChannelManager.PushSyncRequestAsync();

                foreach (var record in diff.Where(a => a.Action == ActionEnum.Add))
                {
                    _logger.LogInformation(LogEvents.Import,
                        "Sending push notification for announcement {id} ({title})", record.Entity.Id,
                        record.Entity.Title);

                    await _pushNotificationChannelManager.PushAnnouncementNotificationAsync(record.Entity);
                }

                _logger.LogInformation(LogEvents.Import, "Announcements import finished successfully.");
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                _logger.LogError(LogEvents.Import, ex, "Job {Name} failed with exception", context.JobDetail.Key.Name);
            }
        }

        private async Task<Guid?> GetImageIdForEntryAsync(string reference, string imageDataBase64)
        {
            reference = $"announcements:{reference}";

            if (string.IsNullOrWhiteSpace(imageDataBase64)) return null;
            var imageBytes = Convert.FromBase64String(imageDataBase64);

            var existingImage = await _imageService.FindAll()
                .FirstOrDefaultAsync(image => image.InternalReference == reference);

            using MemoryStream ms = new(imageBytes);
            if (existingImage != null)
            {
                if (ms.Length != existingImage.SizeInBytes)
                {
                    await _imageService.ReplaceImageAsync(existingImage.Id, reference, ms);
                }
                return existingImage.Id;
            }

            var result = await _imageService.InsertImageAsync(reference, ms);
            return result?.Id;
        }
    }
}