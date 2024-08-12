using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.PushNotifications;

namespace Eurofurence.App.Server.Services.Abstractions.PushNotifications;

public interface IDeviceIdentityService :
    IEntityServiceOperations<DeviceIdentityRecord>,
    IPatchOperationProcessor<DeviceIdentityRecord>
{
    Task<List<DeviceIdentityRecord>> FindByIdentityId(string identityId, CancellationToken cancellationToken = default);
    Task<List<DeviceIdentityRecord>> FindByRegSysId(string regSysId, CancellationToken cancellationToken = default);
}