using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.ArtistsAlley;
using Eurofurence.App.Server.Services.Abstractions.ArtistsAlley;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Eurofurence.App.Server.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/[controller]")]
    public class ArtistsAlleyController : BaseController
    {
        private readonly ITableRegistrationService _tableRegistrationService;

        public ArtistsAlleyController(ITableRegistrationService tableRegistrationService)
        {
            _tableRegistrationService = tableRegistrationService;
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
        public async Task<ActionResult> PostTableRegistrationRequestAsync([EnsureNotNull][FromBody]TableRegistrationRequest request)
        {
            await _tableRegistrationService.RegisterTableAsync(User, request);
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
            var exists = await _tableRegistrationService.HasOneAsync(id);
            if (!exists) return NotFound();

            await _tableRegistrationService.DeleteOneAsync(id);

            return NoContent();
        }
    }
} 