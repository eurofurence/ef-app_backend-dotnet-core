using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eurofurence.App.Common.ExtensionMethods;
using Eurofurence.App.Domain.Model.ArtistsAlley;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions.ArtistsAlley;
using Eurofurence.App.Server.Services.Abstractions.Communication;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Server.Services.Abstractions.Telegram;
using Microsoft.EntityFrameworkCore;

namespace Eurofurence.App.Server.Services.ArtistsAlley
{
    public class TableRegistrationService : ITableRegistrationService
    {
        private readonly AppDbContext _appDbContext;
        private readonly ArtistAlleyConfiguration _configuration;
        private readonly ITelegramMessageSender _telegramMessageSender;
        private readonly IImageService _imageService;
        private readonly IPrivateMessageService _privateMessageService;

        public TableRegistrationService(
            AppDbContext context,
            ArtistAlleyConfiguration configuration,
            ITelegramMessageSender telegramMessageSender,
            IPrivateMessageService privateMessageService,
            IImageService imageService)
        {
            _appDbContext = context;
            _configuration = configuration;
            _telegramMessageSender = telegramMessageSender;
            _privateMessageService = privateMessageService;
            _imageService = imageService;
        }

        public IQueryable<TableRegistrationRecord> GetRegistrations(TableRegistrationRecord.RegistrationStateEnum? state)
        {
            var records = _appDbContext.TableRegistrations.Where(a => !state.HasValue || a.State == state.Value).AsNoTracking();
            return records;
        }

        public async Task RegisterTableAsync(string uid, TableRegistrationRequest request)
        {
            var identity = await _appDbContext.RegSysIdentities.AsNoTracking().FirstOrDefaultAsync(a => a.Uid == uid);

            var imageBytes = Convert.FromBase64String(request.ImageContent);
            var imageFragment = _imageService.GenerateFragmentFromBytes(imageBytes);

            imageFragment = _imageService.EnforceMaximumDimensions(imageFragment, 1500, 1500);

            var record = new TableRegistrationRecord()
            {
                OwnerUid = uid,
                CreatedDateTimeUtc = DateTime.UtcNow,
                DisplayName = request.DisplayName,
                WebsiteUrl = request.WebsiteUrl,
                ShortDescription = request.ShortDescription,
                TelegramHandle = request.TelegramHandle,
                Location = request.Location,
                Image = imageFragment,
                State = TableRegistrationRecord.RegistrationStateEnum.Pending
            };

            record.NewId();
            record.Touch();

            _appDbContext.TableRegistrations.Add(record);
            await _telegramMessageSender.SendMarkdownMessageToChatAsync(
                _configuration.TelegramAdminGroupChatId,
                $"*New Request:* {identity.Username.EscapeMarkdown()} ({uid.EscapeMarkdown()})\n\n*Display Name:* {record.DisplayName.EscapeMarkdown()}\n*Location:* {record.Location.RemoveMarkdown()}\n*Description:* {record.ShortDescription.EscapeMarkdown()}");
            await _appDbContext.SaveChangesAsync();
        }

        public async Task ApproveByIdAsync(Guid id, string operatorUid)
        {
            var record = await _appDbContext.TableRegistrations.FirstOrDefaultAsync(a => a.Id == id
                                                                           && a.State == TableRegistrationRecord.RegistrationStateEnum.Pending);
            var identity = await _appDbContext.RegSysIdentities.AsNoTracking().FirstOrDefaultAsync(a => a.Uid == record.OwnerUid);

            record.ChangeState(TableRegistrationRecord.RegistrationStateEnum.Accepted, operatorUid);
            record.Touch();

            await _telegramMessageSender.SendMarkdownMessageToChatAsync(
                _configuration.TelegramAdminGroupChatId,
                $"*Approved:* {identity.Username.EscapeMarkdown()} ({record.OwnerUid.EscapeMarkdown()} / {record.Id})\n\nRegistration has been approved by *{operatorUid.RemoveMarkdown()}* and will be published on Telegram.");

            await BroadcastAsync(record);

            var message = $"Dear {identity.Username},\n\nWe're happy to inform you that your Artist Alley table registration was accepted as suitable for publication.\n\nA message about your presence in the Artist Alley (along with the text/images you provided) has been posted on our Telegram channel.\n\nFeel free to re-submit the table registration during any other convention day for another signal boost!";

            var sendPrivateMessageRequest = new SendPrivateMessageRequest()
            {
                AuthorName = "Artist Alley",
                RecipientUid = identity.Uid,
                Subject = "Your table registration was accepted",
                Message = message,
                ToastTitle = "Artist Alley",
                ToastMessage = "Your table registration was accepted"
            };

            await _privateMessageService.SendPrivateMessageAsync(sendPrivateMessageRequest);

            await _appDbContext.SaveChangesAsync();
        }

        private async Task BroadcastAsync(TableRegistrationRecord record)
        {
            var telegramMessageBuilder = new StringBuilder();

            telegramMessageBuilder.Append($"Now in the Artist Alley ({record.Location.RemoveMarkdown()}):\n\n*{record.DisplayName.RemoveMarkdown()}*\n\n");

            telegramMessageBuilder.Append(record.ShortDescription.EscapeMarkdown() + "\n\n");

            if (!string.IsNullOrWhiteSpace(record.TelegramHandle))
            {
                telegramMessageBuilder.AppendLine($"Telegram: {record.TelegramHandle.RemoveMarkdown()}"); 
            }
            if (!string.IsNullOrWhiteSpace(record.WebsiteUrl))
            {
                telegramMessageBuilder.AppendLine($"Website: {record.WebsiteUrl.RemoveMarkdown()}");
            }

            var telegramMessage = telegramMessageBuilder.ToString();

            if (record.Image != null)
            {
                await _telegramMessageSender.SendImageToChatAsync(
                    _configuration.TelegramAnnouncementChannelId,
                    record.Image.ImageBytes,
                    telegramMessage);
            }
            else
            {
                await _telegramMessageSender.SendMarkdownMessageToChatAsync(
                    _configuration.TelegramAnnouncementChannelId,
                    telegramMessage);
            }
        }

        public async Task RejectByIdAsync(Guid id, string operatorUid)
        {
            var record = await _appDbContext.TableRegistrations.FirstOrDefaultAsync(a => a.Id == id
                && a.State == TableRegistrationRecord.RegistrationStateEnum.Pending);
            var identity = await _appDbContext.RegSysIdentities.AsNoTracking().FirstOrDefaultAsync(a => a.Uid == record.OwnerUid);

            record.ChangeState(TableRegistrationRecord.RegistrationStateEnum.Rejected, operatorUid);
            record.Touch();

            await _telegramMessageSender.SendMarkdownMessageToChatAsync(_configuration.TelegramAdminGroupChatId,
                $"*Rejected:* {identity.Username.EscapeMarkdown()} ({record.OwnerUid.EscapeMarkdown()} / {record.Id})\n\nRegistration has been rejected by *{operatorUid.RemoveMarkdown()}*.");

            var message = $"Dear {identity.Username},\n\nWe're sorry to inform you that your Artist Alley table registration was considered not suitable for publication.\n\nIt's possible that we couldn't visit your table in time, or that your submitted texts/images are not suitable for public display.\n\nFeel free to update and re-submit the table registration.";

            var sendPrivateMessageRequest = new SendPrivateMessageRequest()
            {
                AuthorName = "Artist Alley",
                RecipientUid = identity.Uid,
                Subject = "Your table registration was rejected",
                Message = message,
                ToastTitle = "Artist Alley",
                ToastMessage = "Your table registration was rejected"
            };

            await _privateMessageService.SendPrivateMessageAsync(sendPrivateMessageRequest);

            await _appDbContext.SaveChangesAsync();
        }

        public async Task<TableRegistrationRecord> GetLatestRegistrationByUidAsync(string uid)
        {
            var request = await _appDbContext.TableRegistrations
                .AsNoTracking()
                .Where(a => a.OwnerUid == uid)
                .OrderByDescending(a => a.CreatedDateTimeUtc)
                .FirstOrDefaultAsync();

            return request;
        }
    }
}