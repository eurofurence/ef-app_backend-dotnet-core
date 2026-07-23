using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using Duende.AspNetCore.Authentication.OAuth2Introspection;
using Eurofurence.App.Domain.Model.ArtistsAlley;
using Eurofurence.App.Domain.Model.Identity;
using Eurofurence.App.Domain.Model.Users;
using Eurofurence.App.Server.Services.Abstractions.Passes;
using Eurofurence.App.Server.Services.Abstractions.Users;
using Eurofurence.App.Server.Web.Extensions;
using Eurofurence.App.Server.Web.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace Eurofurence.App.Server.Web.Controllers
{

    [Route("Api/[controller]")]
    public class UsersController : BaseController
    {
        private readonly IArtistAlleyUserPenaltyService _artistAlleyUserPenaltyService;

        /// <summary>
        /// Provides different kinds of passes to be used in the app or in wallets.
        /// </summary>
        private readonly IPassService _passService;

        private readonly IDistributedCache _cache;
        private const string PassTokenPrefix = "pass-";

        public UsersController(IArtistAlleyUserPenaltyService artistAlleyUserPenaltyService, IPassService passService, IDistributedCache cache)
        {
            _artistAlleyUserPenaltyService = artistAlleyUserPenaltyService;
            _passService = passService;
            _cache = cache;
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
        [Authorize(Roles = $"{IdentityRole.Admin},{IdentityRole.ArtistAlleyAdmin}")]
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
        [Authorize(Roles = $"{IdentityRole.Admin},{IdentityRole.ArtistAlleyAdmin}")]
        public async Task<ArtistAlleyUserPenaltyRecord.PenaltyStatus> GetArtistAlleyUserPenaltyAsync([EnsureNotNull][FromRoute] string id)
        {
            return await _artistAlleyUserPenaltyService.GetUserPenaltyAsync(id);
        }

        /// <summary>
        /// Returns a scannable convention pass for the current user, if they have a valid registration.
        /// </summary>
        /// <param name="mimeType">The MIME type the resulting pass should have. Supported values are: <c>image/svg+xml</c> (default) and <c>application/vnd.apple.pkpass</c></param>
        /// <returns>Convention pass in requested format.</returns>
        [HttpGet("Pass")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [Authorize(AuthenticationSchemes = $"{SingleUseTokenAuthenticationDefaults.AuthenticationScheme},{OAuth2IntrospectionDefaults.AuthenticationScheme}", Roles = IdentityRole.Attendee)]
        public async Task<ActionResult> GetPass([FromQuery] string mimeType = IPassService.MimeTypeSvg)
        {
            if (User.Identity is not ClaimsIdentity identity)
            {
                return Unauthorized();
            }

            PassFile passFile;
            try
            {
                switch (mimeType.ToLower())
                {
                    case IPassService.MimeTypeSvg:
                        passFile = _passService.GenerateSvg(identity);
                        break;
                    case IPassService.MimeTypePkpass:
                        passFile = await _passService.GeneratePkpassAsync(identity);
                        break;
                    default:
                        return BadRequest($"Unsupported MIME type: {mimeType}");
                }
            }
            catch (Exception e) when (e is ArgumentException or FormatException)
            {
                return BadRequest($"Unable to generate pass: {e.Message}");
            }
            if (passFile is not null)
            {
                return File(passFile.Data, passFile.MimeType, passFile.Name);
            }

            return NotFound();
        }

        /// <summary>
        /// Retrieve a single-use token that can be used to download a pass once.
        /// </summary>
        /// <returns>Single-use token that can be used with the <c>Pass</c> endpoint.</returns>
        [HttpGet("Pass/:token")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 404)]
        [Authorize(Roles = IdentityRole.Attendee)]
        public async Task<ActionResult> GetPassToken()
        {
            if (User.Identity is not ClaimsIdentity identity)
            {
                return Unauthorized();
            }

            if (identity.FindFirst("token")?.Value is not { Length: > 0 } identityToken)
            {
                return Unauthorized();
            }

            var claims = new Dictionary<string, string>
            {
                {"token", identityToken},
                {"avatar", identity.Claims.FirstOrDefault(c => c.Type == "avatar")?.Value},
            };

            var token = new SingleUseToken
            {
                ValidUntil = DateTimeOffset.UtcNow.AddMinutes(5),
                PrincipalName = identity.Name,
                Token = $"{PassTokenPrefix}{RandomNumberGenerator.GetHexString(128, true)}",
                Roles = [IdentityRole.Attendee],
                AdditionalClaims = claims
            };

            _cache.SetString(token.Token, JsonSerializer.Serialize(token), new DistributedCacheEntryOptions { AbsoluteExpiration = token.ValidUntil });

            return Ok(token.Token);
        }
    }


}