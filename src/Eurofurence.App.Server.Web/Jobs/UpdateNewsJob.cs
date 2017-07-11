using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Autofac.Features.AttributeFilters;
using Eurofurence.App.Common.DataDiffUtils;
using Eurofurence.App.Common.ExtensionMethods;
using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Server.Services.Abstractions.Announcements;
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
        private readonly IPushEventMediator _pushEventMediator;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public UpdateNewsJob(
            IAnnouncementService announcementService, 
            IPushEventMediator pushEventMediator,
            [KeyFilter("updateNews")] IConfiguration configuration,
            ILoggerFactory loggerFactory)
        {
            _announcementService = announcementService;
            _pushEventMediator = pushEventMediator;
            _configuration = configuration;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public void Execute()
        {
            ExecuteAsync().Wait();
        }

        public async Task ExecuteAsync()
        {
            _logger.LogDebug("Job started");

            var response = string.Empty;
            using (var client = new HttpClient())
            {
                var url = _configuration["source:url"];
                _logger.LogDebug("Fetching data from {url}", url);
                response = await client.GetStringAsync(url);
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
                    Content = j["news"]["message"].Value<string>().RemoveMarkdown(),
                    ValidFromDateTimeUtc = unixReference.AddSeconds(j["date"].Value<double>()).ToUniversalTime(),
                    ValidUntilDateTimeUtc = unixReference
                        .AddSeconds(j["news"]["valid_until"].Value<double>()).ToUniversalTime(),
                },
                Type = j["news"]["type"].Value<string>()
            }).ToList();

            foreach (var item in mapping)
                if (new[] { "new", "reschedule" }.Contains(item.Type))
                    item.Record.ValidUntilDateTimeUtc = item.Record.ValidFromDateTimeUtc.AddHours(48);

            var existingRecords = await _announcementService.FindAllAsync();

            var patch = new PatchDefinition<AnnouncementRecord, AnnouncementRecord>((source, list) =>
                list.SingleOrDefault(a => a.ExternalReference == source.ExternalReference));

            patch
                .Map(s => s.ExternalReference, t => t.ExternalReference)
                .Map(s => s.Area, t => t.Area)
                .Map(s => s.Author, t => t.Author)
                .Map(s => s.Title, t => t.Title)
                .Map(s => s.Content, t => t.Content)
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
        }
    }
}