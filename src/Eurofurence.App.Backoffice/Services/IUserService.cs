using Eurofurence.App.Domain.Model.Users;

namespace Eurofurence.App.Backoffice.Services
{
    public interface IUserService
    {
        public Task<UserRecord> GetUserSelf();
    }
}
