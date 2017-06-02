using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Communication;
using Eurofurence.App.Server.Services.Abstractions;

namespace Eurofurence.App.Server.Services.Communication
{
    public class PrivateMessageService : EntityServiceBase<PrivateMessageRecord>,
        IPrivateMessageService
    {
        public PrivateMessageService(
            IEntityRepository<PrivateMessageRecord> entityRepository,
            IStorageServiceFactory storageServiceFactory
            )
            : base(entityRepository, storageServiceFactory)
        {
        }

        public Task<IEnumerable<PrivateMessageRecord>> GetPrivateMessagesForRecipientAsync(string recipientUid)
        {
            return FindAllAsync(msg => msg.RecipientUid == recipientUid && msg.IsDeleted == 0);
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
            entity.Touch();

            await InsertOneAsync(entity);

            return entity.Id;
        }
    }
}
