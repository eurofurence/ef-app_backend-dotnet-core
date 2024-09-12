using System.Collections.Generic;
using Eurofurence.App.Server.Services.Abstractions.Communication;

namespace Eurofurence.App.Server.Services.Communication
{
    public class PrivateMessageQueueService : IPrivateMessageQueueService
    {
        private readonly Queue<IPrivateMessageQueueService.QueuedNotificationParameters> _notificationQueue = new();

        public void EnqueueMessage(IPrivateMessageQueueService.QueuedNotificationParameters message)
        {
            lock (_notificationQueue)
                _notificationQueue.Enqueue(message);
        }

        public IPrivateMessageQueueService.QueuedNotificationParameters? DequeueMessage()
        {
            lock (_notificationQueue)
            {
                if (_notificationQueue.TryDequeue(out var item))
                    return item;
                return default;
            }
        }

        public int GetQueueSize() {
            lock (_notificationQueue)
                return _notificationQueue.Count;
        }
    }
}