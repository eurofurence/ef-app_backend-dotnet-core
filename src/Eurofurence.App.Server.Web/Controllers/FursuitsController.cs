using System.Threading.Tasks;
using Eurofurence.App.Server.Services.Abstractions.Fursuits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/v2/[controller]")]
    public class FursuitsController : Controller
    {
        private readonly IFursuitBadgeService _fursuitBadgeService;

        public FursuitsController(IFursuitBadgeService fursuitBadgeService)
        {
            _fursuitBadgeService = fursuitBadgeService;
        }

        [Authorize(Roles = "System,Developer,FursuitBadgeSystem")]
        [HttpPost("Badges/Registration")]
        public async Task<ActionResult> PostFursuitBadgeRegistrationAsync([FromBody] FursuitBadgeRegistration registration)
        {
            await _fursuitBadgeService.UpsertFursuitBadgeAsync(registration);
            return Ok();
        }
    }
} 