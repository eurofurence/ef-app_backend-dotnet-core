using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Communication;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Communication;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;
using Microsoft.EntityFrameworkCore;

namespace Eurofurence.App.Server.Services.Communication
{
    public class PrivateMessageService : EntityServiceBase<PrivateMessageRecord>, IPrivateMessageService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IPushNotificationChannelManager _pushNotificationChannelManager;
        private readonly IPrivateMessageQueueService _privateMessageQueueService;

        public PrivateMessageService(
            AppDbContext appDbContext,
            IStorageServiceFactory storageServiceFactory,
            IPushNotificationChannelManager pushNotificationChannelManager,
            IPrivateMessageQueueService privateMessageQueueService
        )
            : base(appDbContext, storageServiceFactory)
        {
            _appDbContext = appDbContext;
            _pushNotificationChannelManager = pushNotificationChannelManager;
            _privateMessageQueueService = privateMessageQueueService;
        }

        public async Task<List<PrivateMessageRecord>> GetPrivateMessagesForRecipientAsync(
            string[] regSysIds,
            string identityId,
            CancellationToken cancellationToken = default)
        {
            var messages = await _appDbContext.PrivateMessages
                .Where(msg =>
                    (regSysIds.Contains(msg.RecipientRegSysId) || msg.RecipientIdentityId == identityId) &&
                    msg.IsDeleted == 0)
                .ToListAsync(cancellationToken);

            if (messages.Count == 0)
            {
                return messages;
            }

            foreach (var message in messages.Where(a => !a.ReceivedDateTimeUtc.HasValue))
            {
                message.ReceivedDateTimeUtc = DateTime.UtcNow;
                message.Touch();
            }

            await _appDbContext.SaveChangesAsync(cancellationToken);

            return messages;
        }

        public async Task<DateTime?> MarkPrivateMessageAsReadAsync(
            Guid messageId,
            string[] regSysIds = null,
            string identityId = null,
            CancellationToken cancellationToken = default)
        {
            var message = await _appDbContext.PrivateMessages
                .Where(pm => pm.Id == messageId && pm.ReadDateTimeUtc == null
                    // Message sent to RegSysId
                    && (string.IsNullOrWhiteSpace(pm.RecipientRegSysId) || (regSysIds != null && regSysIds.Contains(pm.RecipientRegSysId)))
                    // Message sent to IdentityId
                    && (string.IsNullOrWhiteSpace(pm.RecipientIdentityId) || identityId == pm.RecipientIdentityId)
                ).FirstOrDefaultAsync(cancellationToken);
            if (message == null) return null;

            message.ReadDateTimeUtc = DateTime.UtcNow;
            await _appDbContext.SaveChangesAsync(cancellationToken);
            return message.ReadDateTimeUtc;
        }


        private struct QueuedNotificationParameters
        {
            public string RecipientIdentityId;
            public string RecipientRegSysId;
            public string ToastTitle;
            public string ToastMessage;
            public Guid RelatedId;
        }

        public async Task<Guid> SendPrivateMessageAsync(
            SendPrivateMessageByRegSysRequest request,
            string senderUid = "System",
            CancellationToken cancellationToken = default)
        {
            var entity = new PrivateMessageRecord
            {
                AuthorName = request.AuthorName,
                SenderUid = senderUid,
                RecipientRegSysId = request.RecipientUid,
                Message = request.Message,
                Subject = request.Subject,
                CreatedDateTimeUtc = DateTime.UtcNow
            };
            entity.NewId();

            await InsertOneAsync(entity, cancellationToken);

            _privateMessageQueueService.EnqueueMessage(new IPrivateMessageQueueService.QueuedNotificationParameters()
            {
                RecipientRegSysId = request.RecipientUid,
                ToastTitle = request.ToastTitle,
                ToastMessage = request.ToastMessage,
                RelatedId = entity.Id
            });

            return entity.Id;
        }

        public async Task<Guid> SendPrivateMessageAsync(
            SendPrivateMessageByIdentityRequest request,
            string senderUid = "System",
            CancellationToken cancellationToken = default)
        {
            var entity = new PrivateMessageRecord
            {
                AuthorName = request.AuthorName,
                SenderUid = senderUid,
                RecipientIdentityId = request.RecipientUid,
                Message = request.Message,
                Subject = request.Subject,
                CreatedDateTimeUtc = DateTime.UtcNow
            };
            entity.NewId();

            await InsertOneAsync(entity, cancellationToken);

            _privateMessageQueueService.EnqueueMessage(new IPrivateMessageQueueService.QueuedNotificationParameters()
            {
                RecipientIdentityId = request.RecipientUid,
                ToastTitle = request.ToastTitle,
                ToastMessage = request.ToastMessage,
                RelatedId = entity.Id
            });

            return entity.Id;
        }


        private PrivateMessageStatus PrivateMessageRecordToStatus(PrivateMessageRecord message)
        {
            return new PrivateMessageStatus()
            {
                Id = message.Id,
                RecipientRegSysId = message.RecipientRegSysId,
                RecipientIdentityId = message.RecipientIdentityId,
                CreatedDateTimeUtc = message.CreatedDateTimeUtc,
                ReceivedDateTimeUtc = message.ReceivedDateTimeUtc,
                ReadDateTimeUtc = message.ReadDateTimeUtc
            };
        }

        public async Task<PrivateMessageStatus> GetPrivateMessageStatusAsync(
            Guid messageId,
            CancellationToken cancellationToken = default)
        {
            var message = await FindOneAsync(messageId, cancellationToken);
            if (message == null) return null;

            return PrivateMessageRecordToStatus(message);
        }

        public async Task<List<PrivateMessageRecord>> GetPrivateMessagesForSenderAsync(
            string senderUid,
            CancellationToken cancellationToken = default)
        {
            return await _appDbContext.PrivateMessages
                .AsNoTracking()
                .Where(a => a.SenderUid == senderUid)
                .ToListAsync(cancellationToken);
        }

        public async Task<int> FlushPrivateMessageQueueNotifications(
            int messageCount = 10,
            CancellationToken cancellationToken = default)
        {
            var flushedMessageCount = 0;

            for (int i = 0; i < messageCount; i++)
            {
                if (_privateMessageQueueService.DequeueMessage() is { } parameters)
                {
                    if (!string.IsNullOrWhiteSpace(parameters.RecipientRegSysId))
                    {
                        await _pushNotificationChannelManager.PushPrivateMessageNotificationToRegSysIdAsync(
                            parameters.RecipientRegSysId,
                            parameters.ToastTitle,
                            parameters.ToastMessage,
                            parameters.RelatedId,
                            cancellationToken
                        );
                    }
                    else
                    {
                        await _pushNotificationChannelManager.PushPrivateMessageNotificationToIdentityIdAsync(
                            parameters.RecipientIdentityId,
                            parameters.ToastTitle,
                            parameters.ToastMessage,
                            parameters.RelatedId,
                            cancellationToken
                        );
                    }

                    flushedMessageCount++;
                }
                else
                {
                    break;
                }
            }

            return flushedMessageCount;
        }

        public int GetNotificationQueueSize()
        {
            return _privateMessageQueueService.GetQueueSize();
        }
    }
}