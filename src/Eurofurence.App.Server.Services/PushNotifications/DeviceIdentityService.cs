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

public class DeviceIdentityService : EntityServiceBase<DeviceIdentityRecord, DeviceIdentityResponse>, IDeviceIdentityService
{
    private readonly AppDbContext _appDbContext;

    public DeviceIdentityService(
        AppDbContext appDbContext,
        IStorageServiceFactory storageServiceFactory,
        bool useSoftDelete = true
    ) : base(appDbContext, storageServiceFactory, useSoftDelete)
    {
        _appDbContext = appDbContext;
    }

    public Task<List<DeviceIdentityRecord>> FindByIdentityId(
        string identityId,
        CancellationToken cancellationToken = default)
    {
        return _appDbContext.DeviceIdentities
            .Where(x => x.IdentityId == identityId)
            .ToListAsync(cancellationToken);
    }

    public Task<List<DeviceIdentityRecord>> FindByRegSysId(
        string regSysId,
        CancellationToken cancellationToken = default)
    {
        return _appDbContext.Users
            .Where(x => x.RegSysId == regSysId)
            .Join(
                _appDbContext.DeviceIdentities,
                x => x.IdentityId,
                x => x.IdentityId,
                (_, x) => x
            )
            .ToListAsync(cancellationToken);
    }
}