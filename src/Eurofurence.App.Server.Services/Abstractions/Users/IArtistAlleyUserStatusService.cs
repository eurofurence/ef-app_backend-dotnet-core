using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.ArtistsAlley;

namespace Eurofurence.App.Server.Services.Abstractions.Users
{
    public interface IArtistAlleyUserStatusService:
        IEntityServiceOperations<ArtistAlleyUserStatusRecord>,
        IPatchOperationProcessor<ArtistAlleyUserStatusRecord>
    {
        /// <summary>
        /// Changes the status of an applicant (e.g. banning them)
        /// </summary>
        /// <param name="id">Id of the user</param>
        /// <param name="user"></param>
        /// <param name="status">The status that should be set</param>
        /// <param name="reason"></param>
        /// <returns></returns>
        Task SetUserStatusAsync(string id, ClaimsPrincipal user, ArtistAlleyUserStatusRecord.UserStatus status, string reason);

        /// <summary>
        /// Returns the status of an applicant (e.g. banned or ok).
        ///
        /// If the applicant has no status in the database, it is assumed that their status is OK.
        /// TODO: maybe change this, to automatically create a status entry when submitting an application 
        /// </summary>
        /// <param name="id">The Guid of the user</param>
        /// <returns>The status of the user</returns>
        Task<ArtistAlleyUserStatusRecord.UserStatus> GetUserStatusAsync(string id);
        
    }
}