using Eurofurence.App.Domain.Model.Communication;
using Eurofurence.App.Server.Services.Communication;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public interface IPrivateMessageService
    {
        Task<IEnumerable<PrivateMessageRecord>> GetPrivateMessagesForRecipientAsync(string recipientUid);

        Task<bool> MarkPrivateMessageAsReadAsync(Guid messageId, string recipientUid = null);

        Task<Guid> SendPrivateMessageAsync(SendPrivateMessageRequest request);
    }
}
