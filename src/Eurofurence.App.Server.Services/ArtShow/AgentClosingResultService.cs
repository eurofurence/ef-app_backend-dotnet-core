using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.ArtShow;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.ArtShow;
using Eurofurence.App.Server.Services.Abstractions.Communication;

namespace Eurofurence.App.Server.Services.ArtShow
{
    public class AgentClosingResultService : IAgentClosingResultService
    {
        private readonly ConventionSettings _conventionSettings;
        private readonly IEntityRepository<AgentClosingResultRecord> _agentClosingResultRepository;
        private readonly IPrivateMessageService _privateMessageService;
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public AgentClosingResultService(
            ConventionSettings conventionSettings,
            IEntityRepository<AgentClosingResultRecord> agentClosingResultRepository,
            IPrivateMessageService privateMessageService
            )
        {
            _conventionSettings = conventionSettings;
            _agentClosingResultRepository = agentClosingResultRepository;
            _privateMessageService = privateMessageService;
        }

        public async Task ImportAgentClosingResultLogAsync(TextReader logReader)
        {
            var csv = new CsvReader(logReader);

            csv.Configuration.RegisterClassMap<AgentClosingResultImportRowClassMap>();
            csv.Configuration.Delimiter = ",";
            csv.Configuration.HasHeaderRecord = false;

            var csvRecords = csv.GetRecords<AgentClosingResultImportRow>().ToList();

            foreach (var csvRecord in csvRecords)
            {
                var existingRecord = await _agentClosingResultRepository.FindOneAsync(a => a.ImportHash == csvRecord.Hash.Value);
                if (existingRecord != null) continue;

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
                    ImportDateTimeUtc = DateTime.UtcNow,
                    ImportHash = csvRecord.Hash.Value
                };
                newRecord.Touch();

                await _agentClosingResultRepository.InsertOneAsync(newRecord);
            }
        }

        public async Task ExecuteNotificationRunAsync()
        {
            try
            {
                await _semaphore.WaitAsync();

                var newNotifications = await _agentClosingResultRepository.FindAllAsync(a => a.NotificationDateTimeUtc == null);

                var tasks = newNotifications.Select(result => SendAgentClosingResultNotificationAsync(result));

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
                .AppendLine($"Dear {result.AgentName}, as acting agent for {result.ArtistName} we'd like to inform you:\n")
                .AppendLine($"Of {result.ExhibitsTotal} total exhibits, {result.ExhibitsSold} were sold, resulting in a payout of € {result.TotalCashAmount}.\n")
                .AppendLine($"Please pick up any unsold exhibits ({result.ExhibitsUnsold}) and collect the payout in the Art Show during sales hours.\n\nThank you!");

            var request = new SendPrivateMessageRequest()
            {
                AuthorName = "Art Show",
                Subject = title,
                Message = message.ToString(),
                RecipientUid = result.OwnerUid,
                ToastTitle = "Art Show Agent Results",
                ToastMessage = $"{result.ArtistName} ({result.ExhibitsSold} sold, {result.ExhibitsUnsold} unsold to pickup)"
            };

            var privateMessageId = await _privateMessageService.SendPrivateMessageAsync(request);

            result.PrivateMessageId = privateMessageId;
            result.NotificationDateTimeUtc = DateTime.UtcNow;

            await _agentClosingResultRepository.ReplaceOneAsync(result);
        }
    }
}