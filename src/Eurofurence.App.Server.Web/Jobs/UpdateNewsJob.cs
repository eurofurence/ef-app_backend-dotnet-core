﻿using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using Eurofurence.App.Common.DataDiffUtils;
using Eurofurence.App.Common.ExtensionMethods;
using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Server.Services.Abstractions.Announcements;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;
using FluentScheduler;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Eurofurence.App.Server.Web.Jobs
{
    public class UpdateNewsJob : IJob
    {
        private readonly IAnnouncementService _announcementService;
        private readonly IImageService _imageService;
        private readonly IPushEventMediator _pushEventMediator;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public UpdateNewsJob(
            IAnnouncementService announcementService, 
            IImageService imageService,
            IPushEventMediator pushEventMediator,
            [KeyFilter("updateNews")] IConfiguration configuration,
            ILoggerFactory loggerFactory)
        {
            _announcementService = announcementService;
            _imageService = imageService;
            _pushEventMediator = pushEventMediator;
            _configuration = configuration;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public void Execute()
        {
            try
            {
                ExecuteAsync().Wait();
            }
            catch (Exception e)
            {
                _logger.LogError("Job failed with exception {Message} {StackTrace}", e.Message, e.StackTrace);
            }
        }

        public async Task ExecuteAsync()
        {
            _logger.LogDebug("Job started");

            var response = string.Empty;
            using (var client = new HttpClient())
            {
                var url = _configuration["source:url"];
                if (String.IsNullOrWhiteSpace(url))
                {
                    _logger.LogDebug("Empty soruce url; cancelling job", url);
                    return;
                }

                _logger.LogDebug("Fetching data from {url}", url);
                response = await client.GetStringAsync(url);
            }

            if (response == "null")
            {
                _logger.LogDebug("Received null response");
                return;
            }


            var records = JsonConvert.DeserializeObject<JObject[]>(response);
            var unixReference = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

            var mapping = records.Select(j => new
            {
                Record = new AnnouncementRecord()
                {
                    ExternalReference = j["id"].Value<string>(),
                    Area = j["news"]["type"].Value<string>().UppercaseFirst(),
                    Author = j["news"]?["department"]?.Value<string>().UppercaseFirst() ?? "Eurofurence",
                    Title = j["news"]["title"].Value<string>(),
                    Content = j["news"]["message"].Value<string>(),
                    ValidFromDateTimeUtc = unixReference.AddSeconds(j["date"].Value<double>()).ToUniversalTime(),
                    ValidUntilDateTimeUtc = unixReference
                        .AddSeconds(j["news"]["valid_until"].Value<double>()).ToUniversalTime(),
                    ImageId = GetImageIdForEntryAsync(j["id"].Value<string>(), j["data"]?["imagedata"]?.Value<string>()).Result
                },
                Type = j["news"]["type"].Value<string>()
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

            _logger.LogDebug("Diff results in {count} new/modified records", diff.Count);

            if (diff.Count == 0) return;

            _logger.LogInformation("Processing {count} new/modified records", diff.Count);

            await _announcementService.ApplyPatchOperationAsync(diff);
            await _pushEventMediator.PushSyncRequestAsync();

            foreach (var record in diff.Where(a => a.Action == ActionEnum.Add))
            {
                _logger.LogInformation("Sending push notification for announcement {id} ({title})", record.Entity.Id, record.Entity.Title);
                await _pushEventMediator.PushAnnouncementNotificationAsync(record.Entity);
            }

            _logger.LogDebug("Job finished");
        }

        private async Task<Guid?> GetImageIdForEntryAsync(string reference, string imageDataBase64)
        {
            reference = $"announcements:{reference}";

            if (string.IsNullOrWhiteSpace(imageDataBase64)) return null;
            var imageBytes = Convert.FromBase64String(imageDataBase64);

            var image = await _imageService.InsertOrUpdateImageAsync(reference, imageBytes);
            return image.Id;
        }
    }
}