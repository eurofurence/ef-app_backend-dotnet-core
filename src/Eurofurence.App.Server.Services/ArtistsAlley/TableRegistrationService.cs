using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Eurofurence.App.Common.ExtensionMethods;
using Eurofurence.App.Domain.Model.ArtistsAlley;
using Eurofurence.App.Domain.Model.Communication;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.ArtistsAlley;
using Eurofurence.App.Server.Services.Abstractions.Communication;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Eurofurence.App.Server.Services.Abstractions.Telegram;
using Microsoft.EntityFrameworkCore;

namespace Eurofurence.App.Server.Services.ArtistsAlley
{
    public class TableRegistrationService : EntityServiceBase<TableRegistrationRecord>, ITableRegistrationService
    {
        private readonly AppDbContext _appDbContext;
        private readonly ArtistAlleyConfiguration _configuration;
        private readonly ITelegramMessageSender _telegramMessageSender;
        private readonly IImageService _imageService;
        private readonly IPrivateMessageService _privateMessageService;

        public TableRegistrationService(
            AppDbContext context,
            IStorageServiceFactory storageServiceFactory,
            ArtistAlleyConfiguration configuration,
            ITelegramMessageSender telegramMessageSender,
        IPrivateMessageService privateMessageService,
            IImageService imageService) : base(context, storageServiceFactory)
        {
            _appDbContext = context;
            _configuration = configuration;
            _telegramMessageSender = telegramMessageSender;
            _privateMessageService = privateMessageService;
            _imageService = imageService;
        }

        public IQueryable<TableRegistrationRecord> GetRegistrations(TableRegistrationRecord.RegistrationStateEnum? state)
        {
            var records = _appDbContext.TableRegistrations
                .Include(tr => tr.Image)
                .Where(a => !state.HasValue || a.State == state.Value).AsNoTracking();
            return records;
        }

        public override async Task<TableRegistrationRecord> FindOneAsync(Guid id)
        {
            return await _appDbContext.TableRegistrations
                .Include(tr => tr.Image)
                .AsNoTracking()
                .FirstOrDefaultAsync(tr => tr.Id == id);
        }

        public async Task RegisterTableAsync(ClaimsPrincipal user, TableRegistrationRequest request)
        {
            var image = await _imageService.FindOneAsync(request.ImageId);

            await _imageService.EnforceMaximumDimensionsAsync(image, 1500, 1500);

            var record = new TableRegistrationRecord()
            {
                OwnerUid = user.GetSubject(),
                OwnerUsername = user.Identity?.Name,
                CreatedDateTimeUtc = DateTime.UtcNow,
                DisplayName = request.DisplayName,
                WebsiteUrl = request.WebsiteUrl,
                ShortDescription = request.ShortDescription,
                TelegramHandle = request.TelegramHandle,
                Location = request.Location,
                ImageId = request.ImageId,
                State = TableRegistrationRecord.RegistrationStateEnum.Pending
            };

            record.NewId();
            record.Touch();

            _appDbContext.TableRegistrations.Add(record);
            await _telegramMessageSender.SendMarkdownMessageToChatAsync(
                _configuration.TelegramAdminGroupChatId,
                $"*New Request:* {record.OwnerUsername.EscapeMarkdown()} ({user.GetSubject().EscapeMarkdown()})\n\n*Display Name:* {record.DisplayName.EscapeMarkdown()}\n*Location:* {record.Location.RemoveMarkdown()}\n*Description:* {record.ShortDescription.EscapeMarkdown()}");
            await _appDbContext.SaveChangesAsync();
        }

        public async Task ApproveByIdAsync(Guid id, string operatorUid)
        {
            var record = await _appDbContext.TableRegistrations.FirstOrDefaultAsync(a => a.Id == id
                                                                           && a.State == TableRegistrationRecord.RegistrationStateEnum.Pending);
            record.ChangeState(TableRegistrationRecord.RegistrationStateEnum.Accepted, operatorUid);
            record.Touch();

            await _telegramMessageSender.SendMarkdownMessageToChatAsync(
                _configuration.TelegramAdminGroupChatId,
                $"*Approved:* {record.OwnerUsername.EscapeMarkdown()} ({record.OwnerUid.EscapeMarkdown()} / {record.Id})\n\nRegistration has been approved by *{operatorUid.RemoveMarkdown()}* and will be published on Telegram.");

            await BroadcastAsync(record);

            var message = $"Dear {record.OwnerUsername},\n\nWe're happy to inform you that your Artist Alley table registration was accepted as suitable for publication.\n\nA message about your presence in the Artist Alley (along with the text/images you provided) has been posted on our Telegram channel.\n\nFeel free to re-submit the table registration during any other convention day for another signal boost!";

            var sendPrivateMessageRequest = new SendPrivateMessageRequest()
            {
                AuthorName = "Artist Alley",
                RecipientUid = record.OwnerUid,
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
                using (MemoryStream ms = new())
                {
                    var stream = await _imageService.GetImageContentByImageIdAsync(record.Image.Id);
                    await stream.CopyToAsync(ms);
                    await stream.DisposeAsync();
                    await _telegramMessageSender.SendImageToChatAsync(
                        _configuration.TelegramAnnouncementChannelId,
                        ms.ToArray(),
                        telegramMessage);
                }
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

            record.ChangeState(TableRegistrationRecord.RegistrationStateEnum.Rejected, operatorUid);
            record.Touch();

            await _telegramMessageSender.SendMarkdownMessageToChatAsync(_configuration.TelegramAdminGroupChatId,
                $"*Rejected:* {record.OwnerUsername.EscapeMarkdown()} ({record.OwnerUid.EscapeMarkdown()} / {record.Id})\n\nRegistration has been rejected by *{operatorUid.RemoveMarkdown()}*.");

            var message = $"Dear {record.OwnerUsername},\n\nWe're sorry to inform you that your Artist Alley table registration was considered not suitable for publication.\n\nIt's possible that we couldn't visit your table in time, or that your submitted texts/images are not suitable for public display.\n\nFeel free to update and re-submit the table registration.";

            var sendPrivateMessageRequest = new SendPrivateMessageRequest()
            {
                AuthorName = "Artist Alley",
                RecipientUid = record.OwnerUid,
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
                .Include(a => a.Image)
                .AsNoTracking()
                .Where(a => a.OwnerUid == uid)
                .OrderByDescending(a => a.CreatedDateTimeUtc)
                .FirstOrDefaultAsync();

            return request;
        }
    }
}