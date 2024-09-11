using System;
using System.Threading;
using System.Linq;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.PushNotifications;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/[controller]")]
    public class PushNotificationsController : BaseController
    {
        private readonly IPushNotificationChannelManager _pushNotificationChannelManager;
        private readonly IPushNotificationChannelStatisticsService _pushNotificationChannelStatisticsService;

        public PushNotificationsController(
            IPushNotificationChannelManager pushNotificationChannelManager,
            IPushNotificationChannelStatisticsService pushNotificationChannelStatisticsService)
        {
            _pushNotificationChannelManager = pushNotificationChannelManager;
            _pushNotificationChannelStatisticsService = pushNotificationChannelStatisticsService;
        }

        [HttpPost("SyncRequest")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(204)]
        public async Task<ActionResult> PushSyncRequestAsync(CancellationToken cancellationToken = default)
        {
            await _pushNotificationChannelManager.PushSyncRequestAsync(cancellationToken);
            return NoContent();
        }

        [HttpPost("FcmDeviceRegistration")]
        [ProducesResponseType(204)]
        public async Task<ActionResult> PostFcmDeviceRegistrationAsync(
            [FromBody] PostFcmDeviceRegistrationRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request == null) return BadRequest();

            var ids = User.FindAll("RegSysId").Select(x => x.Value).ToArray();

            await _pushNotificationChannelManager.RegisterDeviceAsync(
                request.DeviceId,
                User.GetSubject(),
            ids,
                request.DeviceType,
                cancellationToken
            );
            return NoContent();
        }

        [HttpGet("Statistics")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(PushNotificationChannelStatistics), 200)]
        public async Task<PushNotificationChannelStatistics> GetStatisticsAsync(
            [FromQuery] DateTime? Since,
            CancellationToken cancellationToken = default)
        {
            var result = await _pushNotificationChannelStatisticsService
                .PushNotificationChannelStatisticsAsync(Since, cancellationToken);
            return result;
        }

        public class PostFcmDeviceRegistrationRequest
        {
            public string DeviceId { get; set; }

            public DeviceType DeviceType { get; set; }
        }
    }
}