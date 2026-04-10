using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions.Identity
{
    public interface IIdentityService
    {
        public Task ReadUserInfo(ClaimsIdentity identity);

        public Task ReadRegSys(ClaimsIdentity identity);

        public IEnumerable<string> GetUserGroups(ClaimsIdentity identity);

        public Task<IEnumerable<string>> GetGroupMembers(string groupId);
        public Task<List<string>> GetCachedGroupMembers(string groupId,
        CancellationToken cancellationToken = default);

        /// <summary>
        /// Generates a matrix code for the user based on their reg ID as a svg file.
        /// </summary>
        /// <param name="identity">Users identity</param>
        /// <returns>The svg image data</returns>
        public string GenerateUserMatrixCode(ClaimsIdentity identity);
    }
}
