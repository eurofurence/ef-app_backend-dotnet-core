using System;

namespace Eurofurence.App.Server.Services.Abstractions.Communication
{
    public interface IPrivateMessageQueueService
    {
        void EnqueueMessage(QueuedNotificationParameters message);
        QueuedNotificationParameters? DequeueMessage();
        int GetQueueSize();

        public struct QueuedNotificationParameters
        {
            public string RecipientIdentityId;
            public string RecipientRegSysId;
            public string ToastTitle;
            public string ToastMessage;
            public Guid RelatedId;
        }
    }
}