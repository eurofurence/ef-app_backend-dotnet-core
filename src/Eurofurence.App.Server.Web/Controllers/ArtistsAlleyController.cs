using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.ArtistsAlley;
using Eurofurence.App.Server.Services.Abstractions.ArtistsAlley;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Eurofurence.App.Server.Services.Abstractions.Users;
using Eurofurence.App.Server.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/[controller]")]
    public class ArtistsAlleyController : BaseController
    {
        private readonly ITableRegistrationService _tableRegistrationService;
        private readonly IImageService _imageService;
        private readonly IArtistAlleyUserPenaltyService _artistAlleyUserPenaltyService;
        private readonly ArtistAlleyConfiguration _configuration;

        private const string ArtistAlleyDisabledFilePath = ".ArtistAlleyDisabled";

        public ArtistsAlleyController(ITableRegistrationService tableRegistrationService,ArtistAlleyConfiguration _configuration, IImageService imageService, IArtistAlleyUserPenaltyService artistAlleyUserPenaltyService)
        {
            _tableRegistrationService = tableRegistrationService;
            _imageService = imageService;
            _artistAlleyUserPenaltyService = artistAlleyUserPenaltyService;
            this._configuration = _configuration;
        }
        
        
        [HttpPut("{id}/:status")]
        [Authorize(Roles = "Admin, ArtistAlleyAdmin, ArtistAlleyModerator")]
        public async Task<ActionResult> PutTableRegistrationStatusAsync([EnsureNotNull][FromRoute] Guid id,
            [FromBody] TableRegistrationRecord.RegistrationStateEnum state)
        {
            TableRegistrationRecord record =
                await _tableRegistrationService.GetLatestRegistrationByUidAsync(User.GetSubject());
            // Return 404 if the passed id does not exist
            if (record == null)
            {
                return NotFound();
            }

            if (state == TableRegistrationRecord.RegistrationStateEnum.Rejected)
            {
                await _tableRegistrationService.RejectByIdAsync(id, "API:" + User.Identity.Name);
            }
            else if (state == TableRegistrationRecord.RegistrationStateEnum.Accepted)
            {
                await _tableRegistrationService.ApproveByIdAsync(id, "API:" + User.Identity.Name);
                if (record.ImageId is {} imageId) await _imageService.SetRestricted(imageId, false);
            }

            return NoContent();
        }

        /// <summary>
        ///     Retrieves a list of all table registrations.
        /// </summary>
        /// <returns>All table registrations.</returns>
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(IEnumerable<TableRegistrationRecord>), 200)]
        [Authorize(Roles = "Admin, ArtistAlleyModerator, ArtistAlleyAdmin")]
        [HttpGet]
        public IEnumerable<TableRegistrationRecord> GetTableRegistrationsAsync()
        {
            return _tableRegistrationService.GetRegistrations(null);
        }

        /// <summary>
        ///     Retrieve a single table registration.
        /// </summary>
        /// <param name="id">id of the requested entity</param>
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(TableRegistrationRecord), 200)]
        [Authorize(Roles = "Admin, ArtistAlleyModerator, ArtistAlleyAdmin")]
        [HttpGet("{id}")]
        public async Task<TableRegistrationRecord> GetTableRegistrationAsync(
            [EnsureNotNull][FromRoute] Guid id
        )
        {
            return (await _tableRegistrationService.FindOneAsync(id)).Transient404(HttpContext);
        }

        /// <summary>
        ///     Submits a new table registration for review in the name of the currently signed in user.
        /// </summary>
        /// <param name="request">details of the table registration</param>
        /// <param name="requestImageFile">mandatory image to be attached to the registration</param>
        [Authorize(Roles = "AttendeeCheckedIn")]
        [HttpPost("TableRegistrationRequest")]
        public async Task<ActionResult> PostTableRegistrationRequestAsync([EnsureNotNull][FromForm] TableRegistrationRequest request, [Required] IFormFile requestImageFile)
        {
            if (!_configuration.RegistrationEnabled)
            {
                return StatusCode(403, "Your Artist Alley registration cannot be processed at this time. Please contact the Dealers' Den team about your Artist Alley registration");
            }
            
            if (await _artistAlleyUserPenaltyService.GetUserPenaltyAsync(User.GetSubject()) == ArtistAlleyUserPenaltyRecord.PenaltyStatus.BANNED)
            {
                return StatusCode(403, "Your Artist Alley registration cannot be processed at this time. Please contact the Dealers' Den team about your Artist Alley registration");
            }

            try
            {
                using var imageStream = new MemoryStream();
                if (requestImageFile != null)
                    await requestImageFile.CopyToAsync(imageStream);
                await _tableRegistrationService.RegisterTableAsync(User, request, requestImageFile == null ? null : imageStream);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            return NoContent();
        }

        [Authorize(Roles = "AttendeeCheckedIn")]
        [HttpGet("TableRegistration/:my-latest")]
        public async Task<TableRegistrationRecord> GetMyLatestTableRegistrationRequestAsync()
        {
            var record = await _tableRegistrationService.GetLatestRegistrationByUidAsync(User.GetSubject());
            return record.Transient404(HttpContext);
        }

        /// <summary>
        ///     Delete a table registration..
        /// </summary>
        /// <param name="id">The table registration id.</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<ActionResult> DeleteTableRegistrationAsync([EnsureNotNull][FromRoute] Guid id)
        {
            var tableRegistration = await _tableRegistrationService.FindOneAsync(id);
            if (tableRegistration == null) return NotFound();

            await _tableRegistrationService.DeleteOneAsync(id);

            if (tableRegistration.ImageId is {} imageId) await _imageService.DeleteOneAsync(imageId);

            return NoContent();
        }
    }
}