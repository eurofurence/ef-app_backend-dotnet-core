using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.ArtistsAlley;
using Eurofurence.App.Domain.Model.Users;
using Eurofurence.App.Server.Services.Abstractions.ArtistsAlley;
using Eurofurence.App.Server.Services.Abstractions.Users;
using Eurofurence.App.Server.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/[controller]")]
    public class UsersController : BaseController
    {
        private readonly IArtistAlleyUserPenaltyService _artistAlleyUserPenaltyService;
        public UsersController( IArtistAlleyUserPenaltyService artistAlleyUserPenaltyService)
        {
            _artistAlleyUserPenaltyService = artistAlleyUserPenaltyService;
        }

        [Authorize]
        [HttpGet(":self")]
        [ProducesResponseType(typeof(UserRecord), 200)]
        public UserRecord GetUsersSelf()
        {
            var result = new UserRecord();
            if (User.Identity is ClaimsIdentity identity)
            {
                result.Roles = identity.FindAll(identity.RoleClaimType).Select(c => c.Value).ToArray();
                result.Registrations = identity.FindAll(UserRegistrationClaims.Id).Select(c => {
                    var statusString = identity.FindFirst(UserRegistrationClaims.Status(c.Value))?.Value.Replace(" ", "");
                    Enum.TryParse(statusString, true, out UserRegistrationStatus registrationStatus);
                    var registration = new UserRegistration
                    {
                        Id = c.Value,
                        Status = registrationStatus
                    };
                    return registration;
                }).ToArray();
            }
            return result;
        }

        /// <summary>
        /// Sets (or unsets) a penalty for the artist alley for a given user
        /// </summary>
        /// <param name="id">The identityID of the user</param>
        /// <param name="changeRequest"></param>
        /// <returns></returns>
        [HttpPut("{id}/:artist_alley_penalty")]
        [Authorize(Roles = "Admin,ArtistAlleyAdmin")]
        public async Task<ActionResult> PutArtistAlleyUserPenaltyAsync([EnsureNotNull][FromRoute] string id,
            [Required][FromBody] ArtistAlleyUserPenaltyChangeRequest changeRequest)
        {
            await _artistAlleyUserPenaltyService.SetUserPenaltyAsync(id, User, changeRequest.Penalties, changeRequest.Reason);

            return Ok();
        }

        /// <summary>
        /// Returns (if existing) a penalty status for a user.
        ///
        /// Note that if no record can be found under the passed user id <paramref name="id"/> the status OK (<see cref="ArtistAlleyUserPenaltyRecord.PenaltyStatus.OK"/>) will be returned.
        /// </summary>
        /// <param name="id">The ID of the user</param>
        /// <returns>The penalty status as a string</returns>
        [HttpGet("{id}/:artist_alley_penalty")]
        [Authorize(Roles = "Admin,ArtistAlleyAdmin")]
        public async Task<ArtistAlleyUserPenaltyRecord.PenaltyStatus> GetArtistAlleyUserPenaltyAsync([EnsureNotNull][FromRoute] string id)
        {
            return await _artistAlleyUserPenaltyService.GetUserPenaltyAsync(id);
        }

    }


}