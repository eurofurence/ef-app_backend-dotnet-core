using System;
using System.Linq;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Communication;

namespace Eurofurence.App.Server.Services.Abstractions.Communication
{
    public interface IPrivateMessageService
    {
        Task<IQueryable<PrivateMessageRecord>> GetPrivateMessagesForRecipientAsync(string recipientUid);

        Task<DateTime?> MarkPrivateMessageAsReadAsync(Guid messageId, string recipientUid = null);

        Task<Guid> SendPrivateMessageAsync(SendPrivateMessageRequest request, string senderUid = "System");

        Task<int> FlushPrivateMessageQueueNotifications(int messageCount = 10);

        Task<PrivateMessageStatus> GetPrivateMessageStatusAsync(Guid messageId);

        IQueryable<PrivateMessageRecord> GetPrivateMessagesForSender(string senderUid);

        int GetNotificationQueueSize();
    }
}