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
        private readonly IFirebaseChannelManager _firebaseChannelManager;

        public PushNotificationsController(
            IPushEventMediator pushEventMediator,
            IWnsChannelManager wnsChannelManager,
            IFirebaseChannelManager firebaseChannelManager,
            IApiPrincipal apiPrincipal)
        {
            _firebaseChannelManager = firebaseChannelManager;
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
        [ProducesResponseType(204)]
        public async Task<ActionResult> PostWnsChannelRegistrationAsync(
            [FromBody] PostWnsChannelRegistrationRequest request)
        {
            if (request == null) return BadRequest();

            await _wnsChannelManager.RegisterChannelAsync(request.DeviceId, request.ChannelUri, _apiPrincipal.Uid,
                request.Topics);
            return NoContent();
        }

        [HttpPost("FcmDeviceRegistration")]
        [ProducesResponseType(204)]
        public async Task<ActionResult> PostFcmDeviceRegistrationAsync(
            [FromBody] PostFcmDeviceRegistrationRequest request)
        {
            if (request == null) return BadRequest();

            await _firebaseChannelManager.RegisterDeviceAsync(request.DeviceId, _apiPrincipal.Uid, request.Topics);
            return NoContent();
        }
            
        public class ToastTest
        {
            public string Topic { get; set; }
            public string Message { get; set; }
        }

        public class PostWnsChannelRegistrationRequest
        {
            public string DeviceId { get; set; }
            public string ChannelUri { get; set; }
            public string[] Topics { get; set; }
        }

        public class PostFcmDeviceRegistrationRequest
        {
            public string DeviceId { get; set; }

            public string[] Topics { get; set; }
        }
    }
}