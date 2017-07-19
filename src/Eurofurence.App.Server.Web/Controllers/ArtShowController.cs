using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Eurofurence.App.Server.Services.Abstractions.ArtShow;
using Eurofurence.App.Server.Services.Abstractions.Fursuits;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Eurofurence.App.Server.Web.Extensions;
using Eurofurence.App.Server.Web.Swagger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/v2/[controller]")]
    public class ArtShowController : Controller
    {
        private readonly IItemActivityService _itemActivityService;
        private readonly IApiPrincipal _apiPrincipal;

        public ArtShowController(
            IItemActivityService itemActivityService,
            IApiPrincipal apiPrincipal
            )
        {
            _itemActivityService = itemActivityService;
            _apiPrincipal = apiPrincipal;
        }

        [Authorize(Roles = "System,Developer")]
        [HttpPost("ItemActivites/Log")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ApiErrorResult), 400)]
        [BinaryPayload(Description = "Art show log contents")]
        public async Task<ActionResult> PostFursuitBadgeRegistrationAsync()
        {
            await _itemActivityService.ImportActivityLogAsync(new StreamReader(Request.Body));
            return Ok();
        }
    }
} 