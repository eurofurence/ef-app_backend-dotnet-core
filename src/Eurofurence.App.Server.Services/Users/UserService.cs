using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.PushNotifications;
using Eurofurence.App.Domain.Model.Users;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Microsoft.EntityFrameworkCore;

namespace Eurofurence.App.Server.Services.Users;

public class UserService : EntityServiceBase<UserRecord, UserResponse>, IUserService
{
    private readonly AppDbContext _appDbContext;

    public UserService(
        AppDbContext appDbContext,
        IStorageServiceFactory storageServiceFactory,
        bool useSoftDelete = true
    )
        : base(appDbContext, storageServiceFactory, useSoftDelete)
    {
        _appDbContext = appDbContext;
    }

    public async Task<string> GetOrCreateUserCalendarToken(ClaimsPrincipal user)
    {
        var userRecord = await _appDbContext.Users.FirstOrDefaultAsync(record =>
            record.IdentityId.Equals(user.GetSubject())
        );
        if (userRecord == null)
        {
            return null;
        }

        if (userRecord.CalendarToken == null)
        {
            userRecord.CalendarToken = Guid.NewGuid().ToString();
        }

        await _appDbContext.SaveChangesAsync();

        return userRecord.CalendarToken;
    }
}
