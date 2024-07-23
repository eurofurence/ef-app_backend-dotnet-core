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
        private readonly IMapper _mapper;

        public ArtistsAlleyController(ITableRegistrationService tableRegistrationService, IMapper mapper)
        {
            _tableRegistrationService = tableRegistrationService;
            _mapper = mapper;
        }

        /// <summary>
        ///     Retrieves a list of all table registrations.
        /// </summary>
        /// <returns>All table registrations.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(IEnumerable<TableRegistrationResponse>), 200)]
        [Authorize(Roles = "System,Developer,Admin")]
        [HttpGet]
        public IEnumerable<TableRegistrationResponse> GetTableRegistrationsAsync()
        {
            return _mapper.Map<IEnumerable<TableRegistrationResponse>>(_tableRegistrationService.GetRegistrations(null));
        }

        [Authorize(Roles = "Attendee")]
        [HttpPost("TableRegistrationRequest")]
        public async Task<ActionResult> PostTableRegistrationRequestAsync([EnsureNotNull][FromBody]TableRegistrationRequest Request)
        {
            await _tableRegistrationService.RegisterTableAsync(User, Request);
            return NoContent();
        }

        [Authorize(Roles = "Attendee")]
        [HttpGet("TableRegistration/:my-latest")]
        public async Task<TableRegistrationResponse> GetMyLatestTableRegistrationRequestAsync()
        {
            var record = await _tableRegistrationService.GetLatestRegistrationByUidAsync(User.GetSubject());
            return _mapper.Map<TableRegistrationResponse>(record.Transient404(HttpContext));
        }
    }
} 