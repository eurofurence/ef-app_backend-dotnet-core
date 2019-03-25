using System.Threading.Tasks;
using Eurofurence.App.Server.Services.Abstractions.ArtistsAlley;
using Eurofurence.App.Server.Services.Abstractions.Security;
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
    }
} 