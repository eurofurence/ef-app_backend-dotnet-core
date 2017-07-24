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
        /// <summary>
        ///     Upsert Fursuit Badge information
        /// </summary>
        /// <remarks>
        ///     This is used by the fursuit badge system to push badge information to this backend.
        ///     **Not meant to be consumed by the mobile apps**
        /// </remarks>
        /// <param name="registration"></param>
        [Authorize(Roles = "System,Developer,FursuitBadgeSystem")]
        [HttpPost("Badges/Registration")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ApiErrorResult), 400)]
        public async Task<ActionResult> PostFursuitBadgeRegistrationAsync([FromBody] FursuitBadgeRegistration registration)
        {
            await _fursuitBadgeService.UpsertFursuitBadgeAsync(registration);
            return Ok();
        }

        /// <summary>
        ///     Retrieve the badge image content for a given fursuit badge id
        /// </summary>
        /// <param name="Id">"Id" of the fursuit badge</param>
        [ProducesResponseType(typeof(byte[]), 200)]
        [ProducesResponseType(404)]
        [HttpGet("Badges/{Id}/Image")]
        public async Task<ActionResult> GetFursuitBadgeImageAsync([FromRoute] Guid Id)
        {
            var content = await _fursuitBadgeService.GetFursuitBadgeImageAsync(Id);
            if (content == null) return NotFound();

            return File(content, "image/jpeg");
        }

        /// <summary>
        ///     Register (link/assign) a valid, unused token to a fursuit badge.
        /// </summary>
        /// <param name="FursuitBadgeId"></param>
        /// <param name="TokenValue"></param>
        /// <returns></returns>
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
        [HttpPost("CollectingGame/FursuitParticpation/Badges/{FursuitBadgeId}/Token:safe")]
        [ProducesResponseType(typeof(ApiSafeResult), 200)]
        public async Task<ActionResult> RegisterTokenForFursuitBadgeForOwnerSafeAsync([FromRoute] Guid FursuitBadgeId, [FromBody] string TokenValue)
        {
            var result = await _collectingGameService.RegisterTokenForFursuitBadgeForOwnerAsync(_apiPrincipal.Uid, FursuitBadgeId, TokenValue.ToUpper());
            return result.AsActionResultSafeVariant();
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
        [ProducesResponseType(typeof(PlayerParticipationInfo), 200)]
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


        [Authorize(Roles = "Attendee")]
        [HttpPost("CollectingGame/PlayerParticipation/CollectToken:safe")]
        [ProducesResponseType(typeof(ApiSafeResult<CollectTokenResponse>), 200)]
        public async Task<ActionResult> CollectTokenForPlayerSafeAsync([FromBody] string TokenValue)
        {
            var result = await _collectingGameService.CollectTokenForPlayerAsync(_apiPrincipal.Uid, TokenValue.ToUpper());
            return result.AsActionResultSafeVariant();
        }


        [HttpGet("CollectingGame/PlayerParticipation/Scoreboard")]
        [ProducesResponseType(typeof(PlayerScoreboardEntry[]), 200)]
        [ProducesResponseType(typeof(ApiErrorResult), 400)]
        public async Task<ActionResult> GetPlayerScoreboardEntriesAsync()
        {
            var result = await _collectingGameService.GetPlayerScoreboardEntriesAsync(10);
            return result.AsActionResult();
        }

        [HttpGet("CollectingGame/FursuitParticipation/Scoreboard")]
        [ProducesResponseType(typeof(FursuitScoreboardEntry[]), 200)]
        [ProducesResponseType(typeof(ApiErrorResult), 400)]
        public async Task<ActionResult> GetFursuitScoreboardEntriesAsync()
        {
            var result = await _collectingGameService.GetFursuitScoreboardEntriesAsync(10);
            return result.AsActionResult();
        }

    }
} 