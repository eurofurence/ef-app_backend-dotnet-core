using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Communication;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Communication;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;

namespace Eurofurence.App.Server.Services.Communication
{
    public class PrivateMessageService : EntityServiceBase<PrivateMessageRecord>,
        IPrivateMessageService
    {
        private readonly IPushEventMediator _pushEventMediator;

        public PrivateMessageService(
            IEntityRepository<PrivateMessageRecord> entityRepository,
            IStorageServiceFactory storageServiceFactory,
            IPushEventMediator pushEventMediator
        )
            : base(entityRepository, storageServiceFactory)
        {
            _pushEventMediator = pushEventMediator;
        }

        public async Task<IEnumerable<PrivateMessageRecord>> GetPrivateMessagesForRecipientAsync(string recipientUid)
        {
            var messages = (await FindAllAsync(msg => msg.RecipientUid == recipientUid && msg.IsDeleted == 0)).ToList();

            foreach (var message in messages.Where(a => !a.ReceivedDateTimeUtc.HasValue))
            {
                message.ReceivedDateTimeUtc = DateTime.UtcNow;
                await ReplaceOneAsync(message);
            }

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

        public async Task<Guid> SendPrivateMessageAsync(SendPrivateMessageRequest request)
        {
            var entity = new PrivateMessageRecord
            {
                AuthorName = request.AuthorName,
                RecipientUid = request.RecipientUid,
                Message = request.Message,
                Subject = request.Subject,
                CreatedDateTimeUtc = DateTime.UtcNow
            };
            entity.NewId();

            await InsertOneAsync(entity);
            await _pushEventMediator.PushPrivateMessageNotificationAsync(
                request.RecipientUid, request.ToastTitle, request.ToastMessage);

            return entity.Id;
        }

        public async Task<PrivateMessageStatus> GetPrivateMessageStatusAsync(Guid messageId)
        {
            var message = await FindOneAsync(messageId);
            if (message == null) return null;

            return new PrivateMessageStatus()
            {
                Id = message.Id,
                RecipientUid = message.RecipientUid,
                CreatedDateTimeUtc = message.CreatedDateTimeUtc,
                ReceivedDateTimeUtc = message.ReceivedDateTimeUtc,
                ReadDateTimeUtc = message.ReadDateTimeUtc
            };
        }
    }
}