using Eurofurence.App.Domain.Model.Communication;
using Eurofurence.App.Server.Services.Communication;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public interface IPrivateMessageService
    {
        Task<IEnumerable<PrivateMessageRecord>> GetPrivateMessagesForRecipientAsync(string recipientUid);

        Task<Guid> SendPrivateMessageAsync(SendPrivateMessageRequest request);
    }
}
