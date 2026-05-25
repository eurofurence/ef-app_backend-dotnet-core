using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mime;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.ArtistsAlley;
using Eurofurence.App.Domain.Model.Users;
using Eurofurence.App.Server.Services.Abstractions.Identity;
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

        /// <summary>
        /// Identity service for user identity management.
        /// </summary>
        private readonly IIdentityService _identityService;

        public UsersController(IArtistAlleyUserPenaltyService artistAlleyUserPenaltyService, IIdentityService identityService)
        {
            _artistAlleyUserPenaltyService = artistAlleyUserPenaltyService;
            _identityService = identityService;
        }

        [Authorize]
        [HttpGet(":self")]
        [ProducesResponseType(typeof(UserResponse), 200)]
        public UserResponse GetUsersSelf()
        {
            var result = new UserResponse();
            if (User.Identity is ClaimsIdentity identity)
            {
                result.Roles = identity.FindAll(identity.RoleClaimType).Select(c => c.Value).ToArray();
                result.Registrations = identity.FindAll(UserRegistrationClaims.Id).Select(c =>
                {
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

        /// <summary>
        /// Returns a data matrix code for the current user.
        ///
        /// The code is generated from the reg id of the user and returned as an image.
        /// The image format can be specified by the Accept header.
        /// It should be the same as the code on the con badge.
        /// </summary>
        /// <returns>Data matrix code as svg.</returns>
        [HttpGet("pass")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        [ProducesResponseType(typeof(string), 404)]
        public ActionResult GetDataMatrixCode()
        {
            if (User.Identity is ClaimsIdentity identity)
            {
                if (_identityService.GetRegistrationsIds(identity).Any())
                {
                    try
                    {
                        string fileExtension = MimeTypes.MimeTypeMap.GetExtension(Request.Headers.Accept);
                        return File(
                            Encoding.UTF8.GetBytes(
                                _identityService.GenerateUserMatrixCode(User.Identity as ClaimsIdentity)),
                            Request.Headers.Accept,
                            $"matrix{fileExtension}");
                    }
                    catch (Exception e) when (e is ArgumentException or FormatException)
                    {
                        return BadRequest("Accept header not recognized - unknown format");
                    }
                }
            }
            return NotFound();
        }
    }


}