using System;
using System.Threading.Tasks;
using Eurofurence.App.Server.Services.Abstractions.Fursuits;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Eurofurence.App.Server.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/v2/[controller]")]
    public class FursuitsController : Controller
    {
        private readonly IFursuitBadgeService _fursuitBadgeService;
        private readonly ICollectingGameService _collectingGameService;
        private readonly IApiPrincipal _apiPrincipal;

        public FursuitsController(
            IFursuitBadgeService fursuitBadgeService,
            ICollectingGameService collectingGameService,
            IApiPrincipal apiPrincipal
            )
        {
            _fursuitBadgeService = fursuitBadgeService;
            _collectingGameService = collectingGameService;
            _apiPrincipal = apiPrincipal;
        }

        [Authorize(Roles = "System,Developer,FursuitBadgeSystem")]
        [HttpPost("Badges/Registration")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ApiErrorResult), 400)]
        public async Task<ActionResult> PostFursuitBadgeRegistrationAsync([FromBody] FursuitBadgeRegistration registration)
        {
            await _fursuitBadgeService.UpsertFursuitBadgeAsync(registration);
            return Ok();
        }

        [HttpGet("Badges/{Id}/Image")]
        public async Task<ActionResult> GetFursuitBadgeImageAsync([FromRoute] Guid Id)
        {
            var content = await _fursuitBadgeService.GetFursuitBadgeImageAsync(Id);
            return File(content, "image/jpeg");
        }

        [Authorize(Roles = "Attendee")]
        [HttpPost("CollectingGame/FursuitParticpation/Badges/{FursuitBadgeId}/Token")]
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(ApiErrorResult), 400)]
        public async Task<ActionResult> RegisterTokenForFursuitBadgeForOwnerAsync([FromRoute] Guid FursuitBadgeId, [FromBody] string TokenValue)
        {
            var result = await _collectingGameService.RegisterTokenForFursuitBadgeForOwnerAsync(_apiPrincipal.Uid, FursuitBadgeId, TokenValue.ToUpper());
            return result.AsActionResult();
        }

        [Authorize(Roles = "Attendee")]
        [HttpGet("CollectingGame/FursuitParticipation")]
        [ProducesResponseType(typeof(FursuitParticipationInfo[]), 200)]
        public Task<FursuitParticipationInfo[]> GetFursuitParticipationInfoForOwnerAsync()
        {
            return _collectingGameService.GetFursuitParticipationInfoForOwnerAsync(_apiPrincipal.Uid);
        }

        [Authorize(Roles = "Attendee")]
        [HttpGet("CollectingGame/PlayerParticipation")]
        [ProducesResponseType(typeof(PlayerParticipationInfo[]), 200)]
        public Task<PlayerParticipationInfo> GetPlayerParticipationInfoForPlayerAsync()
        {
            return _collectingGameService.GetPlayerParticipationInfoForPlayerAsync(_apiPrincipal.Uid, _apiPrincipal.GivenName);
        }

        [Authorize(Roles = "Attendee")]
        [HttpPost("CollectingGame/PlayerParticipation/CollectToken")]
        [ProducesResponseType(typeof(CollectTokenResponse), 200)]
        [ProducesResponseType(typeof(ApiErrorResult), 400)]
        public async Task<ActionResult> CollectTokenForPlayerAsync([FromBody] string TokenValue)
        {
            var result = await _collectingGameService.CollectTokenForPlayerAsync(_apiPrincipal.Uid, TokenValue.ToUpper());
            return result.AsActionResult();
        }

        
    }
} 