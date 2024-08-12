using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Communication;

namespace Eurofurence.App.Server.Services.Abstractions.Communication
{
    public interface IPrivateMessageService
    {
        Task<List<PrivateMessageRecord>> GetPrivateMessagesForRecipientAsync(
            string[] regSysId,
            string identityId,
            CancellationToken cancellationToken = default);

        Task<DateTime?> MarkPrivateMessageAsReadAsync(
            Guid messageId,
            string[] regSysIds = null,
            string identityId = null,
            CancellationToken cancellationToken = default);

        Task<Guid> SendPrivateMessageAsync(
            SendPrivateMessageByRegSysRequest request,
            string senderUid = "System",
            CancellationToken cancellationToken = default);
        
        Task<Guid> SendPrivateMessageAsync(
            SendPrivateMessageByIdentityRequest request,
            string senderUid = "System",
            CancellationToken cancellationToken = default);

        Task<int> FlushPrivateMessageQueueNotifications(
            int messageCount = 10,
            CancellationToken cancellationToken = default);

        Task<PrivateMessageStatus> GetPrivateMessageStatusAsync(
            Guid messageId,
            CancellationToken cancellationToken = default);

        Task<List<PrivateMessageRecord>> GetPrivateMessagesForSenderAsync(
            string senderUid,
            CancellationToken cancellationToken = default);

        int GetNotificationQueueSize();
    }
}