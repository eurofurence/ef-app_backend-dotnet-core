using System;
using System.Linq;
using System.Threading;
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
        private readonly IFirebaseChannelManager _firebaseChannelManager;
        private readonly IPushNotificationChannelStatisticsService _pushNotificationChannelStatisticsService;

        public PushNotificationsController(
            IFirebaseChannelManager firebaseChannelManager,
            IPushNotificationChannelStatisticsService pushNotificationChannelStatisticsService)
        {
            _firebaseChannelManager = firebaseChannelManager;
            _pushNotificationChannelStatisticsService = pushNotificationChannelStatisticsService;
        }

        [HttpPost("SyncRequest")]
        [Authorize(Roles = "System,Developer")]
        [ProducesResponseType(204)]
        public async Task<ActionResult> PushSyncRequestAsync(CancellationToken cancellationToken = default)
        {
            await _firebaseChannelManager.PushSyncRequestAsync(cancellationToken);
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

            await _firebaseChannelManager.RegisterDeviceAsync(
                request.DeviceId,
                User.GetSubject(),
                ids,
                request.IsAndroid,
                cancellationToken
            );
            return NoContent();
        }

        [HttpGet("Statistics")]
        [Authorize(Roles = "Developer")]
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

            public bool IsAndroid { get; set; }
        }
    }
}