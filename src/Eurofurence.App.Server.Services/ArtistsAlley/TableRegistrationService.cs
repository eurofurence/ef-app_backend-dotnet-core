using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eurofurence.App.Common.ExtensionMethods;
using Eurofurence.App.Domain.Model;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.ArtistsAlley;
using Eurofurence.App.Domain.Model.Security;
using Eurofurence.App.Server.Services.Abstractions.ArtistsAlley;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Server.Services.Abstractions.Telegram;
using LinqToTwitter;

namespace Eurofurence.App.Server.Services.ArtistsAlley
{
    public class TableRegistrationService : ITableRegistrationService
    {
        private readonly ArtistAlleyConfiguration _configuration;
        private readonly IEntityRepository<TableRegistrationRecord> _tableRegistrationRepository;
        private readonly ITelegramMessageSender _telegramMessageSender;
        private readonly IImageService _imageService;
        private readonly IEntityRepository<RegSysIdentityRecord> _regSysIdentityRepository;
        private TwitterContext _twitterContext;

        public TableRegistrationService(
            ArtistAlleyConfiguration configuration,
            IEntityRepository<TableRegistrationRecord> tableRegistrationRepository,
            ITelegramMessageSender telegramMessageSender,
            IEntityRepository<RegSysIdentityRecord> regSysIdentityRepository,
            IImageService imageService)
        {
            _configuration = configuration;
            _tableRegistrationRepository = tableRegistrationRepository;
            _telegramMessageSender = telegramMessageSender;
            _regSysIdentityRepository = regSysIdentityRepository;
            _imageService = imageService;

            var auth = new SingleUserAuthorizer
            {
                CredentialStore = new SingleUserInMemoryCredentialStore
                {
                    ConsumerKey = _configuration.TwitterConsumerKey,
                    ConsumerSecret = _configuration.TwitterConsumerSecret,
                    AccessToken = _configuration.TwitterAccessToken,
                    AccessTokenSecret = _configuration.TwitterAccessTokenSecret
                }
            };
            _twitterContext = new TwitterContext(auth);
        }

        public async Task<IEnumerable<TableRegistrationRecord>> GetRegistrations(TableRegistrationRecord.RegistrationStateEnum? state)
        {
            var records = await _tableRegistrationRepository.FindAllAsync(a => !state.HasValue || a.State == state.Value);
            return records;
        }

        public async Task RegisterTableAsync(string uid, TableRegistrationRequest request)
        {
            var identity = await _regSysIdentityRepository.FindOneAsync(a => a.Uid == uid);

            byte[] imageBytes = Convert.FromBase64String(request.ImageContent);
            var imageFragment = _imageService.GenerateFragmentFromBytes(imageBytes);

            imageFragment = _imageService.EnforceMaximumDimensions(imageFragment, 1500, 1500);

            var record = new TableRegistrationRecord()
            {
                OwnerUid = uid,
                CreatedDateTimeUtc = DateTime.UtcNow,
                DisplayName = request.DisplayName,
                WebsiteUrl = request.WebsiteUrl,
                ShortDescription = request.ShortDescription,
                Image = imageFragment,
                State = TableRegistrationRecord.RegistrationStateEnum.Pending
            };

            record.NewId();
            record.Touch();

            await _tableRegistrationRepository.InsertOneAsync(record);
            await _telegramMessageSender.SendMarkdownMessageToChatAsync(
                _configuration.TelegramAdminGroupChatId,
                $"*New Table Registration Request:*\n\nFrom: *{identity.Username.RemoveMarkdown()} ({uid})*\n\nDisplay Name: *{record.DisplayName.RemoveMarkdown()}*\n_{record.ShortDescription.RemoveMarkdown()}_");
        }

        public async Task ApproveByIdAsync(Guid id, string operatorUid)
        {
            var record = await _tableRegistrationRepository.FindOneAsync(a => a.Id == id
                && a.State == TableRegistrationRecord.RegistrationStateEnum.Pending);
            var identity = await _regSysIdentityRepository.FindOneAsync(a => a.Uid == record.OwnerUid);

            record.ChangeState(TableRegistrationRecord.RegistrationStateEnum.Accepted, operatorUid);
            record.Touch();

            await _tableRegistrationRepository.ReplaceOneAsync(record);

            await _telegramMessageSender.SendMarkdownMessageToChatAsync(
                _configuration.TelegramAdminGroupChatId,
                $"*Approved:* {identity.Username} ({record.OwnerUid} / {record.Id})\n\nRegistration has been approved by *{operatorUid}* and will be published on Twitter/Telegram.");



            await BroadcastAsync(record);
            // Todo: Send a message to user.
        }

        private async Task BroadcastAsync(TableRegistrationRecord record)
        {
            var messageBuilder = new StringBuilder();
            messageBuilder.Append($"Now in the Artist Alley:\n\n*{record.DisplayName.RemoveMarkdown()}*\n\n");
            messageBuilder.Append($"_{record.ShortDescription.RemoveMarkdown()}_");
            if (record.WebsiteUrl != null)
            {
                messageBuilder.Append("\n\n");
                messageBuilder.Append(record.WebsiteUrl);
            }

            var message = messageBuilder.ToString();

            if (record.Image != null)
            {
                await _telegramMessageSender.SendImageToChatAsync(
                    _configuration.TelegramAnnouncementChannelId,
                    record.Image.ImageBytes,
                    message);

                var media = await _twitterContext.UploadMediaAsync(
                    record.Image.ImageBytes,
                    record.Image.MimeType,
                    "tweet_image");

                await _twitterContext.TweetAsync(message.RemoveMarkdown(), new ulong[] { media.MediaID });
            }
            else
            {
                await _telegramMessageSender.SendMarkdownMessageToChatAsync(
                    _configuration.TelegramAnnouncementChannelId,
                    message);

                await _twitterContext.TweetAsync(message.RemoveMarkdown());
            }
        }

        public async Task RejectByIdAsync(Guid id, string operatorUid)
        {
            var record = await _tableRegistrationRepository.FindOneAsync(a => a.Id == id
                && a.State == TableRegistrationRecord.RegistrationStateEnum.Pending);
            var identity = await _regSysIdentityRepository.FindOneAsync(a => a.Uid == record.OwnerUid);

            record.ChangeState(TableRegistrationRecord.RegistrationStateEnum.Rejected, operatorUid);
            record.Touch();

            await _tableRegistrationRepository.ReplaceOneAsync(record);

            await _telegramMessageSender.SendMarkdownMessageToChatAsync(_configuration.TelegramAdminGroupChatId,
                $"*Rejected:* {identity.Username} ({record.OwnerUid} / {record.Id})\n\nRegistration has been rejected by *{operatorUid}*. Reason given: ...");

            // Todo: Send a message to user. 
        }

        public async Task<TableRegistrationRecord> GetLatestRegistrationByUid(string uid)
        {
            var request = (await _tableRegistrationRepository.FindAllAsync(
                a => a.OwnerUid == uid,
                new FilterOptions<TableRegistrationRecord>()
                    .SortDescending(a => a.CreatedDateTimeUtc)
                    .Take(1)
                )).SingleOrDefault();

            return request;
        }
    }
}