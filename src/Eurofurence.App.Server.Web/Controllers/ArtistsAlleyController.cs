using System.Collections.Generic;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.ArtistsAlley;
using Eurofurence.App.Server.Services.Abstractions.ArtistsAlley;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Eurofurence.App.Server.Web.Extensions;
using MapsterMapper;
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
        [HttpGet]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(IEnumerable<TableRegistrationRecord>), 200)]
        [Authorize(Roles = "System,Developer,Admin")]
        [HttpGet]
        public IEnumerable<TableRegistrationRecord> GetTableRegistrationsAsync()
        {
            return _tableRegistrationService.GetRegistrations(null);
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
    }
} 