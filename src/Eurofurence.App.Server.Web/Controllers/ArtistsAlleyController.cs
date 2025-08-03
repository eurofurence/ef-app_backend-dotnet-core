using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.ArtistsAlley;
using Eurofurence.App.Domain.Model.Transformers;
using Eurofurence.App.Server.Services.Abstractions.ArtistsAlley;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Eurofurence.App.Server.Services.Abstractions.Users;
using Eurofurence.App.Server.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/[controller]")]
    public class ArtistsAlleyController : BaseController
    {
        private readonly ITableRegistrationService _tableRegistrationService;
        private readonly IImageService _imageService;
        private readonly IArtistAlleyUserPenaltyService _artistAlleyUserPenaltyService;
        private readonly ArtistAlleyOptions _artistAlleyOptions;

        public ArtistsAlleyController(ITableRegistrationService tableRegistrationService,
            IOptions<ArtistAlleyOptions> artistAlleyOptions, IImageService imageService,
            IArtistAlleyUserPenaltyService artistAlleyUserPenaltyService)
        {
            _tableRegistrationService = tableRegistrationService;
            _imageService = imageService;
            _artistAlleyUserPenaltyService = artistAlleyUserPenaltyService;
            _artistAlleyOptions = artistAlleyOptions.Value;
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
                if (record.ImageId is { } imageId) await _imageService.SetRestricted(imageId, false);
            }

            return NoContent();
        }

        /// <summary>
        /// Returns a list of all registrations including pending or rejected.
        /// Unlike the regular /ArtistAlley GET endpoint, this endpoint is only accessible to admins and moderators.
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(IEnumerable<TableRegistrationRecord>), 200)]
        [Authorize(Roles = "Admin, ArtistAlleyModerator, ArtistAlleyAdmin")]
        [HttpGet("all")]
        public IEnumerable<TableRegistrationRecord> GetAllTableRegistrationRecords()
        {
            return _tableRegistrationService.GetRegistrations(null);
        }

        /// <summary>
        ///     Retrieves a list of all accepted table registrations.
        /// </summary>
        /// <returns>All table registrations.</returns>
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(IEnumerable<TableRegistrationResponse>), 200)]
        [Authorize(Roles = "Admin, ArtistAlleyModerator, ArtistAlleyAdmin, AttendeeCheckedIn")]
        [HttpGet]
        public IEnumerable<TableRegistrationResponse> GetTableRegistrationsAsync()
        {
            return _tableRegistrationService.GetRegistrations(TableRegistrationRecord.RegistrationStateEnum.Accepted).Select(x => x.Transform());
        }

        /// <summary>
        ///     Retrieve a single table registration.
        /// </summary>
        /// <param name="id">id of the requested entity</param>
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(TableRegistrationResponse), 200)]
        [Authorize(Roles = "Admin, ArtistAlleyModerator, ArtistAlleyAdmin")]
        [HttpGet("{id}")]
        public async Task<TableRegistrationResponse> GetTableRegistrationAsync(
            [EnsureNotNull][FromRoute] Guid id
        )
        {
            return (await _tableRegistrationService.FindOneAsync(id)).Transient404(HttpContext).Transform();
        }

        /// <summary>
        /// Performs a checkout of the user out of the artist alley.
        /// This will delete the registration of the current authenticated user.
        /// </summary>
        /// <returns></returns>
        [Authorize(Roles = "AttendeeCheckedIn")]
        [HttpDelete("TableRegistration/:my-latest")]
        [ProducesResponseType(404)]
        [ProducesResponseType(204)]
        public async Task<ActionResult> ArtistAlleyCheckoutAsync()
        {
            try
            {
                await _tableRegistrationService.DeleteLatestRegistrationByUidAsync(User.GetSubject());
            }
            catch (ArgumentException)
            {
                return NotFound("User has not been registered yet.");
            }
            return NoContent();
        }

        /// <summary>
        /// Converts the provided image file into a MemoryStream.
        /// </summary>
        /// <param name="requestImageFile">The <see cref="IFormFile"/> from the request to convert into a stream</param>
        /// <returns>A stream of the parameter <paramref name="requestImageFile"/></returns>
        private static async Task<MemoryStream> GetImageStreamAsync(IFormFile requestImageFile)
        {
            var imageStream = new MemoryStream();
            if (requestImageFile != null)
                await requestImageFile.CopyToAsync(imageStream);
            return imageStream;
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
            if (!_artistAlleyOptions.RegistrationEnabled)
            {
                return StatusCode(403, "Your Artist Alley registration cannot be processed at this time. Please contact the Dealers' Den team about your Artist Alley registration");
            }

            if (await _artistAlleyUserPenaltyService.GetUserPenaltyAsync(User.GetSubject()) == ArtistAlleyUserPenaltyRecord.PenaltyStatus.BANNED)
            {
                return StatusCode(403, "Your Artist Alley registration cannot be processed at this time. Please contact the Dealers' Den team about your Artist Alley registration");
            }

            TableRegistrationRecord latestRegistrationByUidAsync = await _tableRegistrationService.GetLatestRegistrationByUidAsync(User.GetSubject());

            try
            {
                if (latestRegistrationByUidAsync?.State == TableRegistrationRecord.RegistrationStateEnum.Accepted)
                {
                    // If the user already has an accepted registration, we update it instead of creating a new one.
                    using MemoryStream imageStream = await GetImageStreamAsync(requestImageFile);
                    await _tableRegistrationService.UpdateTableAsync(User, latestRegistrationByUidAsync.Id, request, requestImageFile == null ? null : imageStream);
                }
                else
                {
                    // If the user does not have an accepted registration, we create a new one.
                    using MemoryStream imageStream = await GetImageStreamAsync(requestImageFile);
                    await _tableRegistrationService.RegisterTableAsync(User, request, requestImageFile == null ? null : imageStream);
                }
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }

            return NoContent();
        }

        [Authorize(Roles = "AttendeeCheckedIn")]
        [HttpGet("TableRegistration/:my-latest")]
        public async Task<TableRegistrationResponse> GetMyLatestTableRegistrationRequestAsync()
        {
            var record = await _tableRegistrationService.GetLatestRegistrationByUidAsync(User.GetSubject());
            return record.Transient404(HttpContext)?.Transform();
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

            if (tableRegistration.ImageId is { } imageId) await _imageService.DeleteOneAsync(imageId);

            return NoContent();
        }
    }
}
