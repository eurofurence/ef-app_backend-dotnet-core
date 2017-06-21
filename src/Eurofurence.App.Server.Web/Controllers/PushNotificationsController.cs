using System;
using System.Threading.Tasks;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/v2/[controller]")]
    public class PushNotificationsController : Controller
    {
        private readonly IPushEventMediator _pushEventMediator;
        private readonly IApiPrincipal _apiPrincipal;
        private readonly IWnsChannelManager _wnsChannelManager;

        public PushNotificationsController(
            IPushEventMediator pushEventMediator,
            IWnsChannelManager wnsChannelManager,
            IApiPrincipal apiPrincipal)
        {
            _pushEventMediator = pushEventMediator;
            _apiPrincipal = apiPrincipal;
            _wnsChannelManager = wnsChannelManager;
        }

        [HttpPost("SyncRequest")]
        [Authorize(Roles = "System,Developer")]
        [ProducesResponseType(204)]
        public async Task<ActionResult> PushSyncRequestAsync()
        {
            await _pushEventMediator.PushSyncRequestAsync();
            return NoContent();
        }

        [HttpPost("WnsToast")]
        [Authorize(Roles = "Developer")]
        public async Task<ActionResult> PostWnsToastAsync([FromBody] ToastTest request)
        {
            await _wnsChannelManager.SendToastAsync(request.Topic, request.Message);
            return Ok();
        }

        [HttpPost("WnsChannelRegistration")]
        public async Task<ActionResult> PostWnsChannelRegistrationAsync(
            [FromBody] PostWnsChannelRegistrationRequest request)
        {
            if (request == null) return BadRequest();

            await _wnsChannelManager.RegisterChannelAsync(request.DeviceId, request.ChannelUri, _apiPrincipal.Uid,
                request.Topics);
            return Ok();
        }

        public class ToastTest
        {
            public string Topic { get; set; }
            public string Message { get; set; }
        }

        public class PostWnsChannelRegistrationRequest
        {
            public Guid DeviceId { get; set; }
            public string ChannelUri { get; set; }
            public string[] Topics { get; set; }
        }
    }
}