using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eurofurence.App.Common.ExtensionMethods;
using Eurofurence.App.Domain.Model.ArtistsAlley;
using Eurofurence.App.Domain.Model.Communication;
using Eurofurence.App.Domain.Model.Images;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.ArtistsAlley;
using Eurofurence.App.Server.Services.Abstractions.Communication;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Server.Services.Abstractions.Sanitization;
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
        private readonly IHttpUriSanitizer _uriSanitizer;
        private readonly IHtmlSanitizer _htmlSanitizer;

        public TableRegistrationService(
            AppDbContext context,
            IStorageServiceFactory storageServiceFactory,
            ArtistAlleyConfiguration configuration,
            ITelegramMessageSender telegramMessageSender,
            IPrivateMessageService privateMessageService,
            IImageService imageService,
            IHttpUriSanitizer uriSanitizer,
            IHtmlSanitizer htmlSanitizer) : base(context, storageServiceFactory)
        {
            _appDbContext = context;
            _configuration = configuration;
            _telegramMessageSender = telegramMessageSender;
            _privateMessageService = privateMessageService;
            _imageService = imageService;
            _uriSanitizer = uriSanitizer;
            _htmlSanitizer = htmlSanitizer;
        }

        public IQueryable<TableRegistrationRecord> GetRegistrations(TableRegistrationRecord.RegistrationStateEnum? state)
        {
            var records = _appDbContext.TableRegistrations
                .Include(tr => tr.Image)
                .Where(a => !state.HasValue || a.State == state.Value).AsNoTracking();
            return records;
        }

        public override async Task<TableRegistrationRecord> FindOneAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _appDbContext.TableRegistrations
                .Include(tr => tr.Image)
                .AsNoTracking()
                .FirstOrDefaultAsync(tr => tr.Id == id, cancellationToken);
        }

        public async Task RegisterTableAsync(ClaimsPrincipal user, TableRegistrationRequest request, ImageRecord image = null)
        {
            if (!string.IsNullOrWhiteSpace(request.WebsiteUrl))
                if (_uriSanitizer.Sanitize(request.WebsiteUrl) is string sanitizedUrl and not null)
                    request.WebsiteUrl = sanitizedUrl;
                else
                    throw new ArgumentException("Invalid website URL");

            var subject = user.GetSubject();
            var activeRegistrations = await _appDbContext.TableRegistrations
                .Where(x =>
                    x.OwnerUid == subject &&
                    x.State == TableRegistrationRecord.RegistrationStateEnum.Pending)
                .ToListAsync();

            foreach (var registration in activeRegistrations)
            {
                var stateChange = registration.ChangeState(TableRegistrationRecord.RegistrationStateEnum.Rejected, subject);
                _appDbContext.StateChangeRecord.Add(stateChange);
                registration.Touch();
            }

            var record = new TableRegistrationRecord()
            {
                OwnerUid = user.GetSubject(),
                OwnerUsername = user.Identity?.Name,
                CreatedDateTimeUtc = DateTime.UtcNow,
                DisplayName = _htmlSanitizer.Sanitize(request.DisplayName),
                WebsiteUrl = request.WebsiteUrl,
                ShortDescription = _htmlSanitizer.Sanitize(request.ShortDescription),
                TelegramHandle = request.TelegramHandle,
                Location = request.Location,
                ImageId = image?.Id,
                State = TableRegistrationRecord.RegistrationStateEnum.Pending
            };

            record.NewId();
            record.Touch();

            _appDbContext.TableRegistrations.Add(record);
            await _appDbContext.SaveChangesAsync();
            await _telegramMessageSender.SendMarkdownMessageToChatAsync(
                _configuration.TelegramAdminGroupChatId,
                $"*New Request:* {record.OwnerUsername.EscapeMarkdown()} ({user.GetSubject().EscapeMarkdown()})\n\n*Display Name:* {record.DisplayName.EscapeMarkdown()}\n*Location:* {record.Location.RemoveMarkdown()}\n*Description:* {record.ShortDescription.EscapeMarkdown()}");
        }

        public async Task ApproveByIdAsync(Guid id, string operatorUid)
        {
            var record = await _appDbContext.TableRegistrations.FirstOrDefaultAsync(a => a.Id == id
                                                                           && a.State == TableRegistrationRecord.RegistrationStateEnum.Pending);
            var stateChange =record.ChangeState(TableRegistrationRecord.RegistrationStateEnum.Accepted, operatorUid);
            _appDbContext.StateChangeRecord.Add(stateChange);
            record.Touch();

            await _telegramMessageSender.SendMarkdownMessageToChatAsync(
                _configuration.TelegramAdminGroupChatId,
                $"*Approved:* {record.OwnerUsername.EscapeMarkdown()} ({record.OwnerUid.EscapeMarkdown()} / {record.Id})\n\nRegistration has been approved by *{operatorUid.RemoveMarkdown()}* and will be published on Telegram.");

            await BroadcastAsync(record);

            var message = $"Dear {record.OwnerUsername},\n\nWe're happy to inform you that your Artist Alley table registration was accepted as suitable for publication.\n\nA message about your presence in the Artist Alley (along with the text/images you provided) has been posted on our Telegram channel.\n\nFeel free to re-submit the table registration during any other convention day for another signal boost!";

            var sendPrivateMessageRequest = new SendPrivateMessageByIdentityRequest()
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

            var stateChange = record.ChangeState(TableRegistrationRecord.RegistrationStateEnum.Rejected, operatorUid);
            _appDbContext.StateChangeRecord.Add(stateChange);

            record.Touch();

            await _telegramMessageSender.SendMarkdownMessageToChatAsync(_configuration.TelegramAdminGroupChatId,
                $"*Rejected:* {record.OwnerUsername.EscapeMarkdown()} ({record.OwnerUid.EscapeMarkdown()} / {record.Id})\n\nRegistration has been rejected by *{operatorUid.RemoveMarkdown()}*.");

            var message = $"Dear {record.OwnerUsername},\n\nWe're sorry to inform you that your Artist Alley table registration was considered not suitable for publication.\n\nIt's possible that we couldn't visit your table in time, or that your submitted texts/images are not suitable for public display.\n\nFeel free to update and re-submit the table registration.";

            var sendPrivateMessageRequest = new SendPrivateMessageByRegSysRequest()
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
