using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.PushNotifications;
using Eurofurence.App.Server.Services.Abstractions;

namespace Eurofurence.App.Server.Services.PushNotifications
{
    public class PushNotificiationChannelService : EntityServiceBase<PushNotificationChannelRecord>,
        IPushNotificiationChannelService

    {
        public PushNotificiationChannelService(
            IEntityRepository<PushNotificationChannelRecord> entityRepository,
            IStorageServiceFactory storageServiceFactory
            )
            : base(entityRepository, storageServiceFactory, useSoftDelete: false)
        {
        }
    }
}