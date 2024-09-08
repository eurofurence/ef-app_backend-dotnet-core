using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.ArtistsAlley;

namespace Eurofurence.App.Server.Services.Abstractions.Users
{
    public interface IArtistAlleyUserPenaltyService :
        IEntityServiceOperations<ArtistAlleyUserPenaltyRecord>,
        IPatchOperationProcessor<ArtistAlleyUserPenaltyRecord>
    {
        /// <summary>
        /// Issues a new penalty to a given user or revoking a prior penalty.
        /// </summary>
        /// <param name="id">ID of the user the penalty should affect</param>
        /// <param name="user">The admin user who issued the penalty</param>
        /// <param name="penalties">The penalty status that should be set</param>
        /// <param name="reason">A reason why the penalty was issued</param>
        /// <returns></returns>
        Task SetUserPenaltyAsync(string id, ClaimsPrincipal user, ArtistAlleyUserPenaltyRecord.PenaltyStatus penalties, string reason);

        /// <summary>
        /// Returns an existing penalty of an user.
        /// 
        /// If the user has no record in the database it is assumed that their status is OK (<see cref="ArtistAlleyUserPenaltyRecord.PenaltyStatus.OK"/>).
        /// TODO: maybe change this, to automatically create a status entry when submitting an application 
        /// </summary>
        /// <param name="id">The Guid of the user</param>
        /// <returns>The status of the user</returns>
        Task<ArtistAlleyUserPenaltyRecord.PenaltyStatus> GetUserPenaltyAsync(string id);

    }
}