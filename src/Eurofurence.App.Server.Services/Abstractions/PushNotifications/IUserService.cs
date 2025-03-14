using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.PushNotifications;

namespace Eurofurence.App.Server.Services.Abstractions.PushNotifications;

public interface IUserService :
    IEntityServiceOperations<UserRecord>,
    IPatchOperationProcessor<UserRecord>
{
    /// <summary>
    /// This method will look up an existing calendar API key for the user or create one if not exist.
    ///
    /// This API key can be used to request an .ical calendar from the '/Api/Events/Favorites/calendar.ics' endpoint
    /// with all the users favorite events.
    ///
    /// This token is the only measurement of authenticity for this endpoint.
    /// However, no association with the user or mutability of the calendar is possible from the API. 
    /// </summary>
    /// <param name="user">The user whose API key should be requested</param>
    /// <returns>The API key. Will be null if the passed user <paramref name="user"/> can not be found in the database</returns>
#nullable enable
    Task<string?> GetOrCreateUserCalendarToken(ClaimsPrincipal user);
}