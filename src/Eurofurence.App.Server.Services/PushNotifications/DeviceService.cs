using Eurofurence.App.Domain.Model.PushNotifications;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;

namespace Eurofurence.App.Server.Services.PushNotifications;

public class DeviceService(
    AppDbContext appDbContext,
    IStorageServiceFactory storageServiceFactory,
    bool useSoftDelete = true
) : EntityServiceBase<DeviceRecord>(appDbContext, storageServiceFactory, useSoftDelete), IDeviceService;