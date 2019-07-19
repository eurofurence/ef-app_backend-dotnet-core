using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Eurofurence.App.Server.Services.Abstractions.ArtShow;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Eurofurence.App.Server.Web.Swagger;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/[controller]")]
    public class ArtShowController : BaseController
    {
        private readonly IItemActivityService _itemActivityService;
        private readonly IAgentClosingResultService _agentClosingResultService;
        private readonly IApiPrincipal _apiPrincipal;

        public ArtShowController(
            IItemActivityService itemActivityService,
            IAgentClosingResultService agentClosingResultService,
            IApiPrincipal apiPrincipal
            )
        {
            _itemActivityService = itemActivityService;
            _agentClosingResultService = agentClosingResultService;
            _apiPrincipal = apiPrincipal;
        }

        [Authorize(Roles = "System,Developer")]
        [HttpPost("ItemActivites/Log")]
        [ProducesResponseType(201)]
        [BinaryPayload(Description = "Art show log contents")]
        public async Task<ActionResult> ImportActivityLogAsync()
        {
            await _itemActivityService.ImportActivityLogAsync(new StreamReader(Request.Body));
            return NoContent();
        }

        [Authorize(Roles = "System,Developer")]
        [HttpGet("ItemActivites/NotificationBundles/Simulation")]
        [ProducesResponseType(200)]
        public Task<IList<ItemActivityNotificationResult>> SimulateNotificationRunAsync()
        {
            return _itemActivityService.SimulateNotificationRunAsync();
        }

        [Authorize(Roles = "System,Developer")]
        [HttpPost("ItemActivites/NotificationBundles/Send")]
        [ProducesResponseType(200)]
        public async Task<ActionResult> ExecuteNotificationRunAsync()
        {
            await _itemActivityService.ExecuteNotificationRunAsync();
            return NoContent();
        }

        [Authorize(Roles = "System,Developer")]
        [HttpPost("AgentClosingResults/Log")]
        [ProducesResponseType(201)]
        [BinaryPayload(Description = "Agent closing result log contents")]
        public async Task<ActionResult> ImportAgentClosingResultLogAsync()
        {
            await _agentClosingResultService.ImportAgentClosingResultLogAsync(new StreamReader(Request.Body));
            return NoContent();
        }

        [Authorize(Roles = "System,Developer")]
        [HttpPost("AgentClosingResults/NotificationBundles/Send")]
        [ProducesResponseType(200)]
        public async Task<ActionResult> ExecuteAgentNotificationRunAsync()
        {
            await _agentClosingResultService.ExecuteNotificationRunAsync();
            return NoContent();
        }
    }
} 