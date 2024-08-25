using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.ArtistsAlley;
using Eurofurence.App.Domain.Model.Images;
using Eurofurence.App.Server.Services.Abstractions.ArtistsAlley;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Eurofurence.App.Server.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/[controller]")]
    public class ArtistsAlleyController : BaseController
    {
        private readonly ITableRegistrationService _tableRegistrationService;
        private readonly IImageService _imageService;

        public ArtistsAlleyController(ITableRegistrationService tableRegistrationService, IImageService imageService)
        {
            _tableRegistrationService = tableRegistrationService;
            _imageService = imageService;
        }

        [HttpPut("{id}/:status")]
        [Authorize(Roles = "Admin, ArtistAlleyAdmin")]
        public async Task<ActionResult> PutTableRegistrationStatusAsync([EnsureNotNull] [FromRoute] Guid id,
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
            }

            return NoContent();
        }

        /// <summary>
        ///     Retrieves a list of all table registrations.
        /// </summary>
        /// <returns>All table registrations.</returns>
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(IEnumerable<TableRegistrationRecord>), 200)]
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<TableRegistrationRecord> GetTableRegistrationAsync(
            [EnsureNotNull][FromRoute] Guid id
        )
        {
            return (await _tableRegistrationService.FindOneAsync(id)).Transient404(HttpContext);
        }

        [Authorize(Roles = "Attendee")]
        [HttpPost("TableRegistrationRequest")]
        public async Task<ActionResult> PostTableRegistrationRequestAsync([EnsureNotNull][FromForm] TableRegistrationRequest request, IFormFile requestImageFile)
        {
            ImageRecord image = null;
            if (requestImageFile != null)
            {
                using var ms = new MemoryStream();
                await requestImageFile.CopyToAsync(ms);
                image = await _imageService.InsertImageAsync(requestImageFile.FileName, ms, 1500, 1500);
            }

            if (!Uri.TryCreate(request.WebsiteUrl, UriKind.Absolute, out _)) return BadRequest("Invalid website URL!");

            try
            {
                await _tableRegistrationService.RegisterTableAsync(User, request, image);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            return NoContent();
        }

        [Authorize(Roles = "Attendee")]
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

            if (tableRegistration.ImageId is { } imageId) await _imageService.DeleteOneAsync(imageId);

            return NoContent();
        }
    }
}