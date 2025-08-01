using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions.Identity
{
    public interface IIdentityService
    {
        public Task ReadUserInfo(ClaimsIdentity identity);

        public Task ReadRegSys(ClaimsIdentity identity);

        public IEnumerable<string> GetUserRoles(ClaimsIdentity identity);

        public Task<IEnumerable<string>> GetRoleMembers(ClaimsIdentity identity, string role);
    }
}
