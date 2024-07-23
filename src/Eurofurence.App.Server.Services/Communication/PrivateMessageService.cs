using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Communication;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Communication;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;
using Microsoft.EntityFrameworkCore;

namespace Eurofurence.App.Server.Services.Communication
{
    public class PrivateMessageService : EntityServiceBase<PrivateMessageRecord>,
        IPrivateMessageService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IPushEventMediator _pushEventMediator;
        private readonly ConcurrentQueue<QueuedNotificationParameters> _notificationQueue = new ConcurrentQueue<QueuedNotificationParameters>();

        public PrivateMessageService(
            AppDbContext appDbContext,
            IStorageServiceFactory storageServiceFactory,
            IPushEventMediator pushEventMediator
        )
            : base(appDbContext, storageServiceFactory)
        {
            _appDbContext = appDbContext;
            _pushEventMediator = pushEventMediator;
        }

        public async Task<IQueryable<PrivateMessageRecord>> GetPrivateMessagesForRecipientAsync(string recipientUid)
        {
            var messages = _appDbContext.PrivateMessages.AsNoTracking().Where(msg => msg.RecipientUid == recipientUid && msg.IsDeleted == 0);

            if (!messages.Any())
            {
                return messages;
            }

            foreach (var message in messages.Where(a => !a.ReceivedDateTimeUtc.HasValue))
            {
                message.ReceivedDateTimeUtc = DateTime.UtcNow;
            }

            await ReplaceMultipleAsync(messages);

            return messages;
        }

        public async Task<DateTime?> MarkPrivateMessageAsReadAsync(Guid messageId, string recipientUid = null)
        {
            var message = await FindOneAsync(messageId);
            if (message == null) return null;
            if (!string.IsNullOrWhiteSpace(recipientUid) && message.RecipientUid != recipientUid) return null;

            if (!message.ReadDateTimeUtc.HasValue)
            {
                message.ReadDateTimeUtc = DateTime.UtcNow;
                await ReplaceOneAsync(message);
            }

            return message.ReadDateTimeUtc;
        }


        private struct QueuedNotificationParameters
        {
            public string RecipientUid;
            public string ToastTitle;
            public string ToastMessage;
            public Guid RelatedId;
        }

        public async Task<Guid> SendPrivateMessageAsync(SendPrivateMessageRequest request, string senderUid = "System")
        {
            var entity = new PrivateMessageRecord
            {
                AuthorName = request.AuthorName,
                SenderUid = senderUid,
                RecipientUid = request.RecipientUid,
                Message = request.Message,
                Subject = request.Subject,
                CreatedDateTimeUtc = DateTime.UtcNow
            };
            entity.NewId();

            await InsertOneAsync(entity);

            _notificationQueue.Enqueue(new QueuedNotificationParameters()
            {
                RecipientUid = request.RecipientUid,
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
                RecipientUid = message.RecipientUid,
                CreatedDateTimeUtc = message.CreatedDateTimeUtc,
                ReceivedDateTimeUtc = message.ReceivedDateTimeUtc,
                ReadDateTimeUtc = message.ReadDateTimeUtc
            };
        }

        public async Task<PrivateMessageStatus> GetPrivateMessageStatusAsync(Guid messageId)
        {
            var message = await FindOneAsync(messageId);
            if (message == null) return null;

            return PrivateMessageRecordToStatus(message);
        }

        public IQueryable<PrivateMessageRecord> GetPrivateMessagesForSender(string senderUid)
        {
            return _appDbContext.PrivateMessages.Where(a => a.SenderUid == senderUid);
        }

        public async Task<int> FlushPrivateMessageQueueNotifications(int messageCount = 10)
        {
            var flushedMessageCount = 0;

            for(int i = 0; i < messageCount; i++)
            {
                if (_notificationQueue.TryDequeue(out QueuedNotificationParameters parameters))
                {
                    await _pushEventMediator.PushPrivateMessageNotificationAsync(
                        parameters.RecipientUid,
                        parameters.ToastTitle,
                        parameters.ToastMessage,
                        parameters.RelatedId
                    );

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