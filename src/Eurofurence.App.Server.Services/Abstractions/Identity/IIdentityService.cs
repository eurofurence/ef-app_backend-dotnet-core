using Eurofurence.App.Domain.Model.Users;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions.Identity
{
    public interface IIdentityService
    {
        public Task ReadUserInfo(ClaimsIdentity identity, string token);

        public Task ReadRegSys(ClaimsIdentity identity, string token);

        public Task<UserRegistrationStatus> GetRegistrationStatus(string regSysUrl, string token, string id);
    }
}
