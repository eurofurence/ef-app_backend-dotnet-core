using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.PushNotifications;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;
using Microsoft.EntityFrameworkCore;

namespace Eurofurence.App.Server.Services.PushNotifications;

public class DeviceIdentityService(
    AppDbContext appDbContext,
    IStorageServiceFactory storageServiceFactory,
    bool useSoftDelete = true
) : EntityServiceBase<DeviceIdentityRecord>(appDbContext, storageServiceFactory, useSoftDelete), IDeviceIdentityService
{
    public Task<List<DeviceIdentityRecord>> FindByIdentityId(
        string identityId,
        CancellationToken cancellationToken = default)
    {
        return appDbContext.DeviceIdentities
            .Where(x => x.IdentityId == identityId)
            .ToListAsync(cancellationToken);
    }

    public Task<List<DeviceIdentityRecord>> FindByRegSysId(
        string regSysId,
        CancellationToken cancellationToken = default)
    {
        return appDbContext.RegistrationIdentities
            .Where(x => x.RegSysId == regSysId)
            .Join(
                appDbContext.DeviceIdentities,
                x => x.IdentityId,
                x => x.IdentityId,
                (_, x) => x
            )
            .ToListAsync(cancellationToken);
    }
}