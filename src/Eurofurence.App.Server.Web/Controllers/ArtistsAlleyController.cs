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
        private readonly IApiPrincipal _apiPrincipal;

        public ArtistsAlleyController(
            ITableRegistrationService tableRegistrationService,
            IApiPrincipal apiPrincipal
            )
        {
            _tableRegistrationService = tableRegistrationService;
            _apiPrincipal = apiPrincipal;
        }

        [Authorize(Roles = "Attendee")]
        [HttpPost("TableRegistrationRequest")]
        public async Task<ActionResult> PostTableRegistrationRequestAsync([FromBody]TableRegistrationRequest Request)
        {
            await _tableRegistrationService.RegisterTableAsync(_apiPrincipal.Uid, Request);
            return NoContent();
        }

        [Authorize(Roles = "Attendee")]
        [HttpGet("TableRegistration/:my-latest")]
        public async Task<TableRegistrationRecord> GetMyLatestTableRegistrationRequestAsync()
        {
            var record = await _tableRegistrationService.GetLatestRegistrationByUid(_apiPrincipal.Uid);
            return record.Transient404(HttpContext);
        }
    }
} 