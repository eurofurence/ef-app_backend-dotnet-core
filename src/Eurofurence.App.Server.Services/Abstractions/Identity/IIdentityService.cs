#nullable enable
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions.Identity
{
    public interface IIdentityService
    {
        /// <summary>
        /// Retrieve additional data on user from <c>userinfo</c> endpoint of identity provider and
        /// amend claims accordingly.
        /// </summary>
        /// <param name="identity">Identity for which to fetch userinfo</param>
        /// <returns></returns>
        public Task ReadUserInfo(ClaimsIdentity identity);

        /// <summary>
        /// Retrieve registration ID and status for user from registration system.
        /// </summary>
        /// <param name="identity">Identity for which to fetch registration data</param>
        /// <returns></returns>
        public Task ReadRegSys(ClaimsIdentity identity);

        /// <summary>
        /// Retrieve group claims from identity.
        /// </summary>
        /// <param name="identity">Identity with group claims.</param>
        /// <returns>List of all groups attached to the identity.</returns>
        public IEnumerable<string> GetUserGroups(ClaimsIdentity identity);

        /// <summary>
        /// Retrieve current members of given group from identity provider.
        /// </summary>
        /// <param name="groupId">Group ID for which to fetch member identity IDs</param>
        /// <returns>Identity provider user IDs of all members in group.</returns>
        public Task<IEnumerable<string>> GetGroupMembers(string groupId);

        /// <summary>
        /// Retrieve unexpired, cached identity IDs associated to given group ID.
        /// Groups get cached every time userinfo is refreshed.
        /// Caching is only active if reading group memberships from IDP is not configured.
        /// </summary>
        /// <param name="groupId">Group ID for which to fetch member identity IDs</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Identity provider user IDs of all cached members in group.</returns>
        public Task<List<string>> GetCachedGroupMembers(string groupId,
        CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the registration ID of a user from their claims.
        /// Assumes, that <see cref="ReadRegSys"/> was called before (which it should in the authentication pipeline).
        /// </summary>
        /// <param name="identity">The <see cref="ClaimsIdentity"/> of the user.</param>
        /// <returns>Registration ID of user if present.</returns>
        public string? GetRegistrationId(ClaimsIdentity identity);
    }
}
