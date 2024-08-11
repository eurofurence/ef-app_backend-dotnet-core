using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Fursuits;
using Eurofurence.App.Server.Services.Abstractions.Fursuits;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Eurofurence.App.Server.Web.Extensions;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/[controller]")]
    public class FursuitsController : BaseController
    {
        private readonly IFursuitBadgeService _fursuitBadgeService;
        private readonly ICollectingGameService _collectingGameService;

        public FursuitsController(
            IFursuitBadgeService fursuitBadgeService,
            ICollectingGameService collectingGameService)
        {
            _fursuitBadgeService = fursuitBadgeService;
            _collectingGameService = collectingGameService;
        }
        /// <summary>
        ///     Upsert Fursuit Badge information
        /// </summary>
        /// <remarks>
        ///     This is used by the fursuit badge system to push badge information to this backend.
        ///     **Not meant to be consumed by the mobile apps**
        /// </remarks>
        /// <param name="registration"></param>
        [Authorize(Roles = "Admin,FursuitBadgeSystem")]
        [HttpPost("Badges/Registration")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ApiErrorResult), 400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult> PostFursuitBadgeRegistrationAsync([FromBody] FursuitBadgeRegistration registration)
        {
            await _fursuitBadgeService.UpsertFursuitBadgeAsync(registration);
            return Ok();
        }

        /// <summary>
        ///     Return all Fursuit Badge Registrations
        /// </summary>
        /// <remarks>
        ///     **Not meant to be consumed by the mobile apps**
        /// </remarks>
        [Authorize(Roles = "Admin,FursuitBadgeSystem")]
        [HttpGet("Badges")]
        [ProducesResponseType(typeof(IEnumerable<FursuitBadgeRecord>), 200)]
        [ProducesResponseType(401)]
        public IEnumerable<FursuitBadgeRecord> GetFursuitBadgesAsync(FursuitBadgeFilter filter)
        {
            return _fursuitBadgeService.GetFursuitBadges(filter);
        }

        /// <summary>
        ///     Retrieve the badge image content for a given fursuit badge id
        /// </summary>
        /// <param name="id">"Id" of the fursuit badge</param>
        [ProducesResponseType(typeof(byte[]), 200)]
        [ProducesResponseType(404)]
        [HttpGet("Badges/{id}/Image")]
        [ResponseCache(Duration = 60 * 30, Location = ResponseCacheLocation.Any)]
        public async Task<ActionResult> GetFursuitBadgeImageAsync([FromRoute] Guid id)
        {
            var content = await _fursuitBadgeService.GetFursuitBadgeImageAsync(id);
            if (content == null) return NotFound();

            return File(content, "image/jpeg");
        }

        /// <summary>
        ///     Register (link/assign) a valid, unused token to a fursuit badge.
        /// </summary>
        /// <param name="fursuitBadgeId"></param>
        /// <param name="TokenValue"></param>
        /// <returns></returns>
        [Authorize(Roles = "Attendee")]
        [HttpPost("CollectingGame/FursuitParticpation/Badges/{fursuitBadgeId}/Token")]
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(ApiErrorResult), 400)]
        public async Task<ActionResult> RegisterTokenForFursuitBadgeForOwnerAsync([FromRoute] Guid fursuitBadgeId, [FromBody] string TokenValue)
        {
            var result = await _collectingGameService.RegisterTokenForFursuitBadgeForOwnerAsync(User.GetSubject(), fursuitBadgeId, TokenValue.ToUpper());
            return result.AsActionResult();
        }

        [Authorize(Roles = "Attendee")]
        [HttpPost("CollectingGame/FursuitParticpation/Badges/{fursuitBadgeId}/Token:safe")]
        [ProducesResponseType(typeof(ApiSafeResult), 200)]
        public async Task<ActionResult> RegisterTokenForFursuitBadgeForOwnerSafeAsync([FromRoute] Guid fursuitBadgeId, [FromBody] string TokenValue)
        {
            var result = await _collectingGameService.RegisterTokenForFursuitBadgeForOwnerAsync(User.GetSubject(), fursuitBadgeId, TokenValue.ToUpper());
            return result.AsActionResultSafeVariant();
        }

        [Authorize(Roles = "Attendee")]
        [HttpGet("CollectingGame/FursuitParticipation")]
        [ProducesResponseType(typeof(IEnumerable<FursuitParticipationInfo>), 200)]
        public Task<IEnumerable<FursuitParticipationInfo>> GetFursuitParticipationInfoForOwnerAsync()
        {
            return _collectingGameService.GetFursuitParticipationInfoForOwnerAsync(User.GetSubject());
        }

        [Authorize(Roles = "Attendee")]
        [HttpGet("CollectingGame/PlayerParticipation")]
        [ProducesResponseType(typeof(PlayerParticipationInfo), 200)]
        public Task<PlayerParticipationInfo> GetPlayerParticipationInfoForPlayerAsync()
        {
            return _collectingGameService.GetPlayerParticipationInfoForPlayerAsync(User.GetSubject(), User.Identity!.Name);
        }

        [Authorize(Roles = "Attendee")]
        [HttpGet("CollectingGame/PlayerParticipation/CollectionEntries")]
        [ProducesResponseType(typeof(PlayerCollectionEntry[]), 200)]
        public Task<PlayerCollectionEntry[]> GetPlayerCollectionEntriesForPlayerAsync()
        {
            return _collectingGameService.GetPlayerCollectionEntriesForPlayerAsync(User.GetSubject());
        }

        [Authorize(Roles = "Attendee")]
        [HttpPost("CollectingGame/PlayerParticipation/CollectToken")]
        [ProducesResponseType(typeof(CollectTokenResponse), 200)]
        [ProducesResponseType(typeof(ApiErrorResult), 400)]
        public async Task<ActionResult> CollectTokenForPlayerAsync([FromBody] string TokenValue)
        {
            var result = await _collectingGameService.CollectTokenForPlayerAsync(
                User.GetSubject(),
                TokenValue.Trim().ToUpper(),
                User.GetName()
            );
            return result.AsActionResult();
        }


        [Authorize(Roles = "Attendee")]
        [HttpPost("CollectingGame/PlayerParticipation/CollectToken:safe")]
        [ProducesResponseType(typeof(ApiSafeResult<CollectTokenResponse>), 200)]
        public async Task<ActionResult> CollectTokenForPlayerSafeAsync([FromBody] string TokenValue)
        {
            var result = await _collectingGameService.CollectTokenForPlayerAsync(
                User.GetSubject(),
                TokenValue.Trim().ToUpper(),
                User.GetName()
            );
            return result.AsActionResultSafeVariant();
        }


        [HttpGet("CollectingGame/PlayerParticipation/Scoreboard")]
        [ProducesResponseType(typeof(PlayerScoreboardEntry[]), 200)]
        [ProducesResponseType(typeof(ApiErrorResult), 400)]
        [ResponseCache(Duration = 5, Location = ResponseCacheLocation.Any)]
        public async Task<ActionResult> GetPlayerScoreboardEntriesAsync(int Top = 25)
        {
            Top = Math.Min(Top, 25);
            var result = await _collectingGameService.GetPlayerScoreboardEntriesAsync(Top);
            return result.AsActionResult();
        }

        [HttpGet("CollectingGame/FursuitParticipation/Scoreboard")]
        [ProducesResponseType(typeof(FursuitScoreboardEntry[]), 200)]
        [ProducesResponseType(typeof(ApiErrorResult), 400)]
        [ResponseCache(Duration = 5, Location = ResponseCacheLocation.Any)]
        public async Task<ActionResult> GetFursuitScoreboardEntriesAsync(int Top = 25)
        {
            Top = Math.Min(Top, 25);
            var result = await _collectingGameService.GetFursuitScoreboardEntriesAsync(Top);
            return result.AsActionResult();
        }


        [Authorize(Roles = "Admin")]
        [HttpPost("CollectingGame/Tokens")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ApiErrorResult), 400)]
        public async Task<ActionResult> CreateTokenFromValueAsync([FromBody] string tokenValue)
        {
            return (await _collectingGameService.CreateTokenFromValueAsync(tokenValue))
                .AsActionResult();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("CollectingGame/Tokens/Batch")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ApiErrorResult), 400)]
        public async Task<ActionResult> CreateTokensFromValuesAsync([FromBody] string[] tokenValues)
        {
            return (await _collectingGameService.CreateTokensFromValuesAsync(tokenValues))
                .AsActionResult();
        }


        [Authorize(Roles = "Admin")]
        [HttpPost("CollectingGame/Recalculate")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ApiErrorResult), 400)]
        public async Task<ActionResult> RecalculateAsync()
        {
            return (await _collectingGameService.RecalculateAsync()).AsActionResult();
        }

    }
} 