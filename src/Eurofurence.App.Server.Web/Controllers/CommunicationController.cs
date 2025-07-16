using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Communication;
using Eurofurence.App.Domain.Model.Transformers;
using Eurofurence.App.Server.Services.Abstractions.Communication;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Eurofurence.App.Server.Web.Extensions;
using Eurofurence.App.Server.Web.Identity;
using IdentityModel.AspNetCore.OAuth2Introspection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/[controller]")]
    public class CommunicationController : BaseController
    {
        private readonly IPrivateMessageService _privateMessageService;

        public CommunicationController(IPrivateMessageService privateMessageService)
        {
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
        [ProducesResponseType(typeof(IEnumerable<PrivateMessageResponse>), 200)]
        public async Task<IEnumerable<PrivateMessageResponse>> GetMyPrivateMessagesAsync(CancellationToken cancellationToken = default)
        {
            return (await _privateMessageService.GetPrivateMessagesForRecipientAsync(
                User.GetRegSysIds(),
                User.GetSubject(),
                cancellationToken
            )).Select(x => x.Transform());
        }

        /// <summary>
        ///     Marks a given private message as read (reading receipt).
        /// </summary>
        /// <remarks>
        ///     Calling this on a message that has already been marked as read
        ///     will not update the `ReadDateTimeUtc` property, but return the
        ///     `ReadDateTimeUtc` value of the first call.
        /// </remarks>
        /// <param name="messageId">`Id` of the message to mark as read</param>
        /// <param name="isRead">boolean, expected to be 'true' always</param>
        /// <param name="cancellationToken">Token for request cancellation</param>
        /// <returns>The current timestamp on the server that will be persisted in the messages `ReadDateTimeUtc` property.</returns>
        /// <response code="400">`MessageId` is invalid or not accessible by the user.</response>
        [Authorize(Roles = "Attendee")]
        [HttpPost("PrivateMessages/{messageId}/Read")]
        [ProducesResponseType(typeof(DateTime), 200)]
        public async Task<ActionResult> MarkMyPrivateMessageAsReadAsync(
            [EnsureNotNull][FromRoute] Guid messageId,
            [EnsureNotNull][FromBody] bool isRead,
            CancellationToken cancellationToken = default
        )
        {
            if (!isRead) return BadRequest("Message can only be marked as read; not as unread.");

            var result = await _privateMessageService.MarkPrivateMessageAsReadAsync(
                messageId,
                User.GetRegSysIds(),
                User.GetSubject(),
                cancellationToken
            );
            return result.HasValue ? Json(result) : BadRequest();
        }

        /// <summary>
        ///     Sends a private message to a specific recipient/attendee.
        /// </summary>
        /// <remarks>
        ///     If the backend has a push-channel available to any given device(s) that are currently signed into the app
        ///     with the same recipient uid, it will push a toast message to those devices.
        ///     The toast message content is defined by the `ToastTitle` and `ToastMessage` properties.
        /// </remarks>
        /// <param name="request"></param>
        /// <param name="cancellationToken">Token for Cancelling the operation.</param>
        /// <returns>The `ID` of the message that has been delivered.</returns>
        /// <response code="400">Unable to parse `Request`</response>
        [Authorize(AuthenticationSchemes = $"{ApiKeyAuthenticationDefaults.AuthenticationScheme},{OAuth2IntrospectionDefaults.AuthenticationScheme}", Roles = "Admin,PrivateMessageSender")]
        [HttpPost("PrivateMessages")]
        [HttpPost("PrivateMessages/:byRegistrationId")]
        [ProducesResponseType(typeof(Guid), 200)]
        public async Task<ActionResult> SendPrivateMessageAsync(
            [FromBody] SendPrivateMessageByRegSysRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null) return BadRequest();

            // Only admins may set the AuthorName via the API
            if (!User.IsInRole("Admin") || string.IsNullOrWhiteSpace(request.AuthorName))
            {
                request.AuthorName = User.GetName();
            }

            return Json(await _privateMessageService.SendPrivateMessageAsync(
                request,
                User.GetSubject(),
                cancellationToken
            ));
        }

        /// <summary>
        ///     Sends a private message to a specific recipient/attendee.
        /// </summary>
        /// <remarks>
        ///     If the backend has a push-channel available to any given device(s) that are currently signed into the app
        ///     with the same recipient uid, it will push a toast message to those devices.
        ///     The toast message content is defined by the `ToastTitle` and `ToastMessage` properties.
        /// </remarks>
        /// <param name="request">Private message to be sent.</param>
        /// <param name="cancellationToken">Token for Cancelling the operation.</param>
        /// <returns>The `ID` of the message that has been delivered.</returns>
        /// <response code="400">Unable to parse `Request`</response>
        [Authorize(AuthenticationSchemes = $"{ApiKeyAuthenticationDefaults.AuthenticationScheme},{OAuth2IntrospectionDefaults.AuthenticationScheme}", Roles = "Admin,PrivateMessageSender")]
        [HttpPost("PrivateMessages/:byIdentityId")]
        [ProducesResponseType(typeof(Guid), 200)]
        public async Task<ActionResult> SendPrivateMessageIdentityAsync(
            [FromBody] SendPrivateMessageByIdentityRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null) return BadRequest();

            // Only admins may set the AuthorName via the API
            if (!User.IsInRole("Admin") || string.IsNullOrWhiteSpace(request.AuthorName))
            {
                request.AuthorName = User.GetName();
            }

            return Json(await _privateMessageService.SendPrivateMessageAsync(
                request,
                User.GetSubject(),
                cancellationToken
            ));
        }

        [HttpGet("PrivateMessages/{messageId}/Status")]
        [ProducesResponseType(typeof(PrivateMessageStatus), 200)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<PrivateMessageStatus> GetPrivateMessageStatusAsync(
            [FromRoute][EnsureNotNull] Guid messageId,
            CancellationToken cancellationToken = default)
        {
            var result = await _privateMessageService.GetPrivateMessageStatusAsync(messageId, cancellationToken);
            return result.Transient404(HttpContext);
        }

        [HttpGet("PrivateMessages/:sent-by-me")]
        [Authorize]
        [ProducesResponseType(typeof(IEnumerable<PrivateMessageResponse>), 200)]
        public async Task<IEnumerable<PrivateMessageResponse>> GetMySentPrivateMessagesAsync(
            CancellationToken cancellationToken = default)
        {
            return (await _privateMessageService.GetPrivateMessagesForSenderAsync(User.GetSubject(), cancellationToken)).Select(x => x.Transform());
        }

        [HttpGet("NotificationQueue/Count")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(int), 200)]
        public int GetNotificationQueueSize()
        {
            return _privateMessageService.GetNotificationQueueSize();
        }
    }
}