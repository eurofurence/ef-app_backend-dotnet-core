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

        private readonly ITableRegistrationService _tableRegistrationService;

        private readonly IArtistAlleyUserStatusService _artistAlleyUserStatusService;
        public UsersController(ITableRegistrationService tableRegistrationService, IArtistAlleyUserStatusService artistAlleyUserStatusService)
        {
            _tableRegistrationService = tableRegistrationService;
            _artistAlleyUserStatusService = artistAlleyUserStatusService;
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

        [HttpPut("{id}/:artist_alley_status")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> PutTableRegistrationUserStatusAsync([EnsureNotNull][FromRoute] string id,
            [Required][FromBody] ArtistAlleyUserStatusChangeRequest changeRequest)
        {
            await _artistAlleyUserStatusService.SetUserStatusAsync(id, User, changeRequest.Status, changeRequest.Reason);

            return NoContent();
        }


        [HttpGet("{id}/:artist_alley_status")]
        [Authorize(Roles = "Admin")]
        public async Task<ArtistAlleyUserStatusRecord.UserStatus> GetTableRegistrationUserStatusAsync([EnsureNotNull][FromRoute] string id)
        {

            return await _artistAlleyUserStatusService.GetUserStatusAsync(id);
        }

    }


}