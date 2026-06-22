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
        /// Generates a data matrix code for the user based on their registration as SVG.
        /// </summary>
        /// <param name="registrationId">Registration id of the user.</param>
        /// <returns>The SVG image data</returns>
        public string GenerateDataMatrixCode(string registrationId);

        /// <summary>
        /// Finds all registrations ids of the user.
        /// Assumes, that <see cref="ReadRegSys"/> was called before (which it should in the authentication pipeline).
        /// </summary>
        /// <param name="identity">The <see cref="ClaimsIdentity"/> of the user.</param>
        /// <returns>A sequence of all reg id of the user.</returns>
        public IEnumerable<string> GetRegistrationsIds(ClaimsIdentity identity);
    }
}
