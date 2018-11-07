using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Communication;

namespace Eurofurence.App.Server.Services.Abstractions.Communication
{
    public interface IPrivateMessageService
    {
        Task<IEnumerable<PrivateMessageRecord>> GetPrivateMessagesForRecipientAsync(string recipientUid);

        Task<DateTime?> MarkPrivateMessageAsReadAsync(Guid messageId, string recipientUid = null);

        Task<Guid> SendPrivateMessageAsync(SendPrivateMessageRequest request, string senderUid = "System");

        Task<int> FlushPrivateMessageQueueNotifications(int messageCount = 10);

        Task<PrivateMessageStatus> GetPrivateMessageStatusAsync(Guid messageId);

        Task<IEnumerable<PrivateMessageRecord>> GetPrivateMessagesForSenderAsync(string senderUid);
    }
}