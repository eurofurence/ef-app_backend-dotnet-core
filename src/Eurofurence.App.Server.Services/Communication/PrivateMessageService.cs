using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Communication;
using Eurofurence.App.Server.Services.Abstractions;
using System.Linq;

namespace Eurofurence.App.Server.Services.Communication
{
    public class PrivateMessageService : EntityServiceBase<PrivateMessageRecord>,
        IPrivateMessageService
    {
        readonly IWnsChannelManager _wnsChannelManager;

        public PrivateMessageService(
            IEntityRepository<PrivateMessageRecord> entityRepository,
            IStorageServiceFactory storageServiceFactory,
            IWnsChannelManager wnsChannelManager
            )
            : base(entityRepository, storageServiceFactory)
        {
            _wnsChannelManager = wnsChannelManager;
        }

        public async Task<IEnumerable<PrivateMessageRecord>> GetPrivateMessagesForRecipientAsync(string recipientUid)
        {
            var messages = (await FindAllAsync(msg => msg.RecipientUid == recipientUid && msg.IsDeleted == 0)).ToList();

            foreach(var message in messages.Where(a => !a.ReceivedDateTimeUtc.HasValue))
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
            if (!String.IsNullOrWhiteSpace(recipientUid) && message.RecipientUid != recipientUid) return null;

            if (!message.ReadDateTimeUtc.HasValue)
            {
                message.ReadDateTimeUtc = DateTime.UtcNow;
                await ReplaceOneAsync(message);
            }

            return message.ReadDateTimeUtc;
        }

        public async Task<Guid> SendPrivateMessageAsync(SendPrivateMessageRequest request)
        {
            var entity = new PrivateMessageRecord()
            {
                AuthorName = request.AuthorName,
                RecipientUid = request.RecipientUid,
                Message = request.Message,
                Subject = request.Subject,
                CreatedDateTimeUtc = DateTime.UtcNow
            };
            entity.NewId();

            await InsertOneAsync(entity);
            await _wnsChannelManager.PushPrivateMessageNotificationAsync(request.RecipientUid);

            return entity.Id;
        }
    }
}
