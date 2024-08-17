using Eurofurence.App.Domain.Model.Users;

namespace Eurofurence.App.Backoffice.Services
{
    public interface IUsersService
    {
        public Task<UserRecord> GetUserSelf();
    }
}
