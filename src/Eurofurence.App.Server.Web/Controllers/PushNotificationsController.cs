using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Security;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/v2/[controller]")]
    public class PushNotificationsController : Controller
    {
        readonly IWnsChannelManager _wnsChannelManager;
        readonly ApiPrincipal _apiPrincipal;

        public PushNotificationsController(
            IWnsChannelManager wnsChannelManager, 
            ApiPrincipal apiPrincipal)
        {
            _apiPrincipal = apiPrincipal;
            _wnsChannelManager = wnsChannelManager;
        }

        public class PostWnsChannelRegistrationRequest
        {
            public Guid DeviceId { get; set; }
            public string ChannelUri { get; set; }
            public string[] Topics { get; set; }
        }

        [HttpPost("WnsChannelRegistration")]
        public async Task<ActionResult> PostWnsChannelRegistrationAsync([FromBody] PostWnsChannelRegistrationRequest request)
        {
            if (request == null) return BadRequest();

            await _wnsChannelManager.RegisterChannelAsync(request.DeviceId, request.ChannelUri, _apiPrincipal.Uid, request.Topics);
            return Ok();
        }

    }
}
