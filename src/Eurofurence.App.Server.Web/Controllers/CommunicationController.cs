using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Communication;
using Eurofurence.App.Server.Services.Abstractions.Communication;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Eurofurence.App.Server.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/v2/[controller]")]
    public class CommunicationController : Controller
    {
        private readonly IApiPrincipal _apiPrincipal;
        private readonly IPrivateMessageService _privateMessageService;

        public CommunicationController(IPrivateMessageService privateMessageService, IApiPrincipal apiPrincipal)
        {
            _apiPrincipal = apiPrincipal;
            _privateMessageService = privateMessageService;
        }

        /// <summary>
        ///     Retrieves all private messages of an authenticated attendee.
        /// </summary>
        /// <remarks>
        ///     This will set the `ReceivedDateTimeUtc` to the current server time on all messages retrieved
        ///     that have not been retrieved in a previous call.
        /// </remarks>
        /// <returns>A list of all private messages for the authorized attendee</returns>
        [Authorize(Roles = "Attendee")]
        [HttpGet("PrivateMessages")]
        [ProducesResponseType(typeof(IEnumerable<PrivateMessageRecord>), 200)]
        public Task<IEnumerable<PrivateMessageRecord>> GetMyPrivateMessagesAsync()
        {
            return _privateMessageService.GetPrivateMessagesForRecipientAsync(_apiPrincipal.Uid);
        }

        /// <summary>
        ///     Marks a given private message as read (reading receipt).
        /// </summary>
        /// <remarks>
        ///     Calling this on a message that has already been marked as read
        ///     will not update the `ReadDateTimeUtc` property, but return the
        ///     `ReadDateTimeUtc` value of the first call.
        /// </remarks>
        /// <param name="MessageId">`Id` of the message to mark as read</param>
        /// <param name="IsRead">boolean, expected to be 'true' always</param>
        /// <returns>The current timestamp on the server that will be persisted in the messages `ReadDateTimeUtc` property.</returns>
        /// <response code="400">`MessageId` is invalid or not accessible by the user.</response>
        [Authorize(Roles = "Attendee")]
        [HttpPost("PrivateMessages/{MessageId}/Read")]
        [ProducesResponseType(typeof(DateTime), 200)]
        public async Task<ActionResult> MarkMyPrivateMessageAsReadAsync(
            [EnsureNotNull][FromRoute] Guid MessageId,
            [EnsureNotNull][FromBody] bool IsRead
        )
        {
            if (!IsRead) return BadRequest("Message can only be marked as read; not as unread.");

            var result = await _privateMessageService.MarkPrivateMessageAsReadAsync(MessageId, _apiPrincipal.Uid);
            return result.HasValue ? (ActionResult) Json(result) : BadRequest();
        }

        /// <summary>
        ///     Sends a private message to a specific recipient/attendee.
        /// </summary>
        /// <remarks>
        ///     If the backend has a push-channel available to any given device(s) that are currently signed into the app
        ///     with the same recipient uid, it will push a toast message to those devices.
        ///     The toast message content is defined by the `ToastTitle` and `ToastMessage` properties.
        /// </remarks>
        /// <param name="Request"></param>
        /// <returns>The `Id` of the message that has been delivered.</returns>
        /// <response code="400">Unable to parse `Request`</response>
        [Authorize(Roles = "Developer,System,Action-PrivateMessages-Send")]
        [HttpPost("PrivateMessages")]
        [ProducesResponseType(typeof(Guid), 200)]
        public async Task<ActionResult> SendPrivateMessageAsync([FromBody] SendPrivateMessageRequest Request)
        {
            if (Request == null) return BadRequest();

            return Json(await _privateMessageService.SendPrivateMessageAsync(Request));
        }


        [HttpGet("PrivateMessages/{MessageId}/Status")]
        [Authorize(Roles = "Developer,System,Action-PrivateMessages-Query")]
        [ProducesResponseType(typeof(PrivateMessageStatus), 200)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<PrivateMessageStatus> GetPrivateMessageStatusAsync(
            [FromRoute][EnsureNotNull] Guid MessageId)
        {
            var result = await _privateMessageService.GetPrivateMessageStatusAsync(MessageId);
            return result.Transient404(HttpContext);
        }

    }
}