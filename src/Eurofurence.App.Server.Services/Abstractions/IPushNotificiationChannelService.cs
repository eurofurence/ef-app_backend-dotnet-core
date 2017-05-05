using Eurofurence.App.Domain.Model.PushNotifications;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public interface IPushNotificiationChannelService :
        IEntityServiceOperations<PushNotificationChannelRecord>,
        IPatchOperationProcessor<PushNotificationChannelRecord>
    {

    }
}