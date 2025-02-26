using System.Security.Claims;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.PushNotifications;

namespace Eurofurence.App.Server.Services.Abstractions.PushNotifications;

public interface IRegistrationIdentityService :
    IEntityServiceOperations<UserRecord>,
    IPatchOperationProcessor<UserRecord>
{
    Task<string> GetOrCreateUserCalendarToken(ClaimsPrincipal user);
}