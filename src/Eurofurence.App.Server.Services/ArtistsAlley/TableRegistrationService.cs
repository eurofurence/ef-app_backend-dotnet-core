﻿using System;
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
using Eurofurence.App.Server.Services.Abstractions.Communication;
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
        private readonly IPrivateMessageService _privateMessageService;
        private TwitterContext _twitterContext;

        public TableRegistrationService(
            ArtistAlleyConfiguration configuration,
            IEntityRepository<TableRegistrationRecord> tableRegistrationRepository,
            ITelegramMessageSender telegramMessageSender,
            IEntityRepository<RegSysIdentityRecord> regSysIdentityRepository,
            IPrivateMessageService privateMessageService,
            IImageService imageService)
        {
            _configuration = configuration;
            _tableRegistrationRepository = tableRegistrationRepository;
            _telegramMessageSender = telegramMessageSender;
            _regSysIdentityRepository = regSysIdentityRepository;
            _privateMessageService = privateMessageService;
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
                TelegramHandle = request.TelegramHandle,
                Location = request.Location,
                Image = imageFragment,
                State = TableRegistrationRecord.RegistrationStateEnum.Pending
            };

            record.NewId();
            record.Touch();

            await _tableRegistrationRepository.InsertOneAsync(record);
            await _telegramMessageSender.SendMarkdownMessageToChatAsync(
                _configuration.TelegramAdminGroupChatId,
                $"*New Request:* {identity.Username.EscapeMarkdown()} ({uid.EscapeMarkdown()})\n\n*Display Name:* {record.DisplayName.EscapeMarkdown()}\n*Description:* {record.ShortDescription.EscapeMarkdown()}");
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
                $"*Approved:* {identity.Username.EscapeMarkdown()} ({record.OwnerUid.EscapeMarkdown()} / {record.Id})\n\nRegistration has been approved by *{operatorUid.RemoveMarkdown()}* and will be published on Twitter/Telegram.");

            await BroadcastAsync(record);

            // Todo: Send a message to user.
        }

        private async Task BroadcastAsync(TableRegistrationRecord record)
        {
            var telegramMessageBuilder = new StringBuilder();
            var twitterMessageBuilder = new StringBuilder();

            telegramMessageBuilder.Append($"Now in the Artist Alley ({record.Location.EscapeMarkdown()}):\n\n*{record.DisplayName.RemoveMarkdown()}*\n\n");
            twitterMessageBuilder.Append($"Now in the Artist Alley ({record.Location}):\n\n{record.DisplayName}\n\n");

            telegramMessageBuilder.Append(record.ShortDescription.EscapeMarkdown() + "\n\n");
            twitterMessageBuilder.Append(record.ShortDescription + "\n\n");

            if (!string.IsNullOrWhiteSpace(record.TelegramHandle))
            {
                telegramMessageBuilder.AppendLine($"Telgram: {record.TelegramHandle.RemoveMarkdown()}"); 
                twitterMessageBuilder.AppendLine($"Telgram: https://t.me/{record.TelegramHandle.Replace("@", "")}");
            }
            if (!string.IsNullOrWhiteSpace(record.WebsiteUrl))
            {
                telegramMessageBuilder.AppendLine($"Website: {record.WebsiteUrl.RemoveMarkdown()}");
                twitterMessageBuilder.AppendLine($"Website: {record.WebsiteUrl}");
            }

            var twitterMessage = twitterMessageBuilder.ToString();
            var telegramMessage = telegramMessageBuilder.ToString();

            if (record.Image != null)
            {
                await _telegramMessageSender.SendImageToChatAsync(
                    _configuration.TelegramAnnouncementChannelId,
                    record.Image.ImageBytes,
                    telegramMessage);

                var media = await _twitterContext.UploadMediaAsync(
                    record.Image.ImageBytes,
                    record.Image.MimeType,
                    "tweet_image");

                await _twitterContext.TweetAsync(twitterMessage, new ulong[] { media.MediaID });
            }
            else
            {
                await _telegramMessageSender.SendMarkdownMessageToChatAsync(
                    _configuration.TelegramAnnouncementChannelId,
                    telegramMessage);

                await _twitterContext.TweetAsync(twitterMessage);
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
                $"*Rejected:* {identity.Username.EscapeMarkdown()} ({record.OwnerUid.EscapeMarkdown()} / {record.Id})\n\nRegistration has been rejected by *{operatorUid.RemoveMarkdown()}*.");

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