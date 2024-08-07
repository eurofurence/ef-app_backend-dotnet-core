using System;
using System.Threading.Tasks;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Eurofurence.App.Server.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/[controller]")]
    public class PushNotificationsController : BaseController
    {
        private readonly IPushEventMediator _pushEventMediator;
        private readonly IWnsChannelManager _wnsChannelManager;
        private readonly IFirebaseChannelManager _firebaseChannelManager;
        private readonly IPushNotificationChannelStatisticsService _pushNotificationChannelStatisticsService;

        public PushNotificationsController(
            IPushEventMediator pushEventMediator,
            IWnsChannelManager wnsChannelManager,
            IFirebaseChannelManager firebaseChannelManager,
            IPushNotificationChannelStatisticsService pushNotificationChannelStatisticsService)
        {
            _firebaseChannelManager = firebaseChannelManager;
            _pushEventMediator = pushEventMediator;
            _wnsChannelManager = wnsChannelManager;
            _pushNotificationChannelStatisticsService = pushNotificationChannelStatisticsService;
        }

        [HttpPost("SyncRequest")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(204)]
        public async Task<ActionResult> PushSyncRequestAsync()
        {
            await _pushEventMediator.PushSyncRequestAsync();
            return NoContent();
        }

        [HttpPost("WnsToast")]
        [Authorize(Roles = "Admin")]
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

            await _wnsChannelManager.RegisterChannelAsync(request.DeviceId, request.ChannelUri, User.GetSubject(),
                request.Topics);
            return NoContent();
        }

        [HttpPost("FcmDeviceRegistration")]
        [ProducesResponseType(204)]
        public async Task<ActionResult> PostFcmDeviceRegistrationAsync(
            [FromBody] PostFcmDeviceRegistrationRequest request)
        {
            if (request == null) return BadRequest();

            await _firebaseChannelManager.RegisterDeviceAsync(request.DeviceId, User.GetSubject(), request.Topics);
            return NoContent();
        }

        [HttpGet("Statistics")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(PushNotificationChannelStatistics), 200)]
        public async Task<PushNotificationChannelStatistics> GetStatisticsAsync([FromQuery] DateTime? Since)
        {
            var result = await _pushNotificationChannelStatisticsService.PushNotificationChannelStatisticsAsync(Since);
            return result;
        }


        [HttpPut("Topics/{topic}/{deviceId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ApiErrorResult), 400)]
        public async Task<ActionResult> SubscribeToTopicAsync(string topic, string deviceId)
        {
            var result = await _firebaseChannelManager.SubscribeToTopicAsync(deviceId, topic);
            return result.AsActionResult();
        }


        [HttpDelete("Topics/{topic}/{deviceId}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ApiErrorResult), 400)]
        public async Task<ActionResult> UnsubscribeFromTopicAsync(string topic, string deviceId)
        {
            var result = await _firebaseChannelManager.UnsubscribeFromTopicAsync(deviceId, topic);
            return result.AsActionResult();
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