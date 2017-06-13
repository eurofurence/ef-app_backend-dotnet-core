using Eurofurence.App.Domain.Model.Communication;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Communication;
using Eurofurence.App.Server.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/v2/[controller]")]
    public class CommunicationController : Controller
    {
        readonly IPrivateMessageService _privateMessageService;
        readonly ApiPrincipal _apiPrincipal;

        public CommunicationController(IPrivateMessageService privateMessageService, ApiPrincipal apiPrincipal)
        {
            _apiPrincipal = apiPrincipal;
            _privateMessageService = privateMessageService;
        }

        [Authorize(Roles="Attendee")]
        [HttpGet("PrivateMessages")]
        public Task<IEnumerable<PrivateMessageRecord>> GetMyPrivateMessagesAsync()
        {
            return _privateMessageService.GetPrivateMessagesForRecipientAsync(_apiPrincipal.Uid);
        }

        [Authorize(Roles = "Attendee")]
        [HttpPost("PrivateMessages/{messageId}/Read")]
        public async Task<ActionResult> MarkMyPrivateMessageAsReadAsync([FromRoute] Guid messageId)
        {
            var result = await _privateMessageService.MarkPrivateMessageAsReadAsync(messageId, _apiPrincipal.Uid);
            return result ? (ActionResult)NoContent() : (ActionResult)BadRequest();
        }

        [Authorize(Roles = "Developer")]
        [HttpPost("PrivateMessages")]
        public Task<Guid> SendPrivateMessageAsync([FromBody] SendPrivateMessageRequest request)
        {
            return _privateMessageService.SendPrivateMessageAsync(request);
        }

    }
}
