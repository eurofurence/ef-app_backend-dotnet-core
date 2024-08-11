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
        private readonly IFirebaseChannelManager _firebaseChannelManager;

        private readonly ConcurrentQueue<QueuedNotificationParameters> _notificationQueue =
            new ConcurrentQueue<QueuedNotificationParameters>();

        public PrivateMessageService(
            AppDbContext appDbContext,
            IStorageServiceFactory storageServiceFactory,
            IFirebaseChannelManager firebaseChannelManager
        )
            : base(appDbContext, storageServiceFactory)
        {
            _appDbContext = appDbContext;
            _firebaseChannelManager = firebaseChannelManager;
        }

        public async Task<List<PrivateMessageRecord>> GetPrivateMessagesForRecipientAsync(
            string[] regSysIds,
            string identityId,
            CancellationToken cancellationToken = default)
        {
            var messages = await _appDbContext.PrivateMessages.AsNoTracking()
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
            }

            await ReplaceMultipleAsync(messages, cancellationToken);

            return messages;
        }

        public async Task<DateTime?> MarkPrivateMessageAsReadAsync(
            Guid messageId,
            string[] regSysIds = null,
            string identityId = null,
            CancellationToken cancellationToken = default)
        {
            var message = await FindOneAsync(messageId, cancellationToken);
            if (message == null) return null;
            if (
                (regSysIds is not null && regSysIds.Contains(message.RecipientRegSysId)) ||
                (!string.IsNullOrWhiteSpace(identityId) && message.RecipientIdentityId != identityId))
            {
                return null;
            }

            if (!message.ReadDateTimeUtc.HasValue)
            {
                message.ReadDateTimeUtc = DateTime.UtcNow;
                await ReplaceOneAsync(message, cancellationToken);
            }

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

            _notificationQueue.Enqueue(new QueuedNotificationParameters()
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

            _notificationQueue.Enqueue(new QueuedNotificationParameters()
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
                if (_notificationQueue.TryDequeue(out QueuedNotificationParameters parameters))
                {
                    if (!string.IsNullOrWhiteSpace(parameters.RecipientRegSysId))
                    {
                        await _firebaseChannelManager.PushPrivateMessageNotificationToRegSysIdAsync(
                            parameters.RecipientRegSysId,
                            parameters.ToastTitle,
                            parameters.ToastMessage,
                            parameters.RelatedId,
                            cancellationToken
                        );
                    }
                    else
                    {
                        await _firebaseChannelManager.PushPrivateMessageNotificationToIdentityIdAsync(
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
            return _notificationQueue.Count;
        }
    }
}