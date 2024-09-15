using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Eurofurence.App.Server.Services.Abstractions.ArtShow;
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

        public ArtShowController(
            IItemActivityService itemActivityService,
            IAgentClosingResultService agentClosingResultService)
        {
            _itemActivityService = itemActivityService;
            _agentClosingResultService = agentClosingResultService;
        }

        /// <summary>
        /// Import Art Show results for bidders (CSV)
        /// </summary>
        /// <remarks>
        /// Imports the Art Show result CSV for bidders (`RegNo,ASIDNO,ArtistName,ArtPieceTitle,Status,FinalBidAmount`), where `Status` may only be one of `Auction` or `Sold`.<br /><br />
        /// There's a row-hash built from the mentioned import fields. If a row with the same hash is already present, it will be skipped.
        /// (Importing the same data multiple times does not lead to duplicates.)
        /// </remarks> 
        [Authorize(Roles = "Admin,ArtShow")]
        [HttpPost("ItemActivites/Log")]
        [ProducesResponseType(201)]
        [BinaryPayload(Description = "Art show log contents")]
        public Task<ImportResult> ImportActivityLogAsync()
        {
            return _itemActivityService.ImportActivityLogAsync(new StreamReader(Request.Body));
        }

        /// <summary>
        /// Simulate notification bundle generation for bidders.
        /// </summary>
        /// <remarks>
        /// Simulates building notification bundles that group all item results (sold, to auction) by the bidder who holds the winning/highest bid. <br /><br />
        /// The simulation data shows individual notifications for every bidder (`RecipientUid`), listing the items they won (`IdsSold`)
        /// as well as items going into auction (`IdsToAuction`) with their current bid on top. <br /><br />
        /// <b>Calling this endpoint does not modify any data and does not send any messages.</b>
        /// </remarks> 
        [Authorize(Roles = "Admin,ArtShow")]
        [HttpGet("ItemActivites/NotificationBundles/Simulation")]
        [ProducesResponseType(200)]
        public IList<ItemActivityNotificationResult> SimulateItemActivitiesNotificationRunAsync()
        {
            return _itemActivityService.SimulateNotificationRun();
        }


        /// <summary>
        /// Execute/Send notification bundles for bidders.
        /// </summary>
        /// <remarks>
        /// Builds notification bundles that group all item results (sold, to auction) by the bidder who holds the winning/highest bid. <br /><br />
        /// <b><u>Calling this endpoint will produce and queue all notifications/messages for delivery. Notifications are sent asynchronously and in queue,
        /// so full delivery may take a few minutes.</u></b>
        /// </remarks> 
        [Authorize(Roles = "Admin,ArtShow")]
        [HttpPost("ItemActivites/NotificationBundles/Send")]
        [ProducesResponseType(204)]
        public async Task<ActionResult> ExecuteNotificationRunAsync()
        {
            await _itemActivityService.ExecuteNotificationRunAsync();
            return NoContent();
        }


        /// <summary>
        /// Delete unprocessed rows
        /// </summary>
        /// <remarks>
        /// Deletes any imported data for which no notification has been sent yet.<br /><br />
        /// <b>Use this to remove imported data if the simulation does not check out.</b>
        /// </remarks>
        [Authorize(Roles = "Admin,ArtShow")]
        [HttpDelete("ItemActivites/NotificationBundles/:unprocessed")]
        [ProducesResponseType(204)]
        public async Task<ActionResult> DeleteUnprocessedItemActivitiesImportRowsAsync()
        {
            await _itemActivityService.DeleteUnprocessedImportRowsAsync();
            return NoContent();
        }


        /// <summary>
        /// Import Art Show results for agents (CSV)
        /// </summary>
        /// <remarks>
        /// Imports the Art Show result CSV for agents (`AgentBadgeNo,AgentName,ArtistName,TotalCashAmount,ExhibitsSold,ExhibitsUnsold,ExhibitsToAuction`).<br /><br />
        /// There's a row-hash built from the mentioned import fields. If a row with the same hash is already present, it will be skipped.
        /// (Importing the same data multiple times does not lead to duplicates.)
        /// </remarks> 
        [Authorize(Roles = "Admin,ArtShow")]
        [HttpPost("AgentClosingResults/Log")]
        [ProducesResponseType(201)]
        [BinaryPayload(Description = "Agent closing result log contents")]
        public Task<ImportResult> ImportAgentClosingResultLogAsync()
        {
            return _agentClosingResultService.ImportAgentClosingResultLogAsync(new StreamReader(Request.Body));
        }

        /// <summary>
        /// Simulate notification bundle generation for agents.
        /// </summary>
        /// <remarks>
        /// Simulates building notification going towards agents. <br /><br />
        /// The simulation data shows individual notification data for every agent (`AgentBadgeNo`).<br /><br />
        /// <b>Calling this endpoint does not modify any data and does not send any messages.</b>
        /// </remarks> 
        [Authorize(Roles = "Admin,ArtShow")]
        [HttpGet("AgentClosingResults/NotificationBundles/Simulation")]
        [ProducesResponseType(200)]
        public IList<AgentClosingNotificationResult> SimulateAgentClosingResultsNotificationRunAsync()
        {
            return _agentClosingResultService.SimulateNotificationRun();
        }


        /// <summary>
        /// Execute/Send notification bundles for agents.
        /// </summary>
        /// <remarks>
        /// Builds notifications for all agents.<br /><br />
        /// <b><u>Calling this endpoint will produce and queue all notifications/messages for delivery. Notifications are sent asynchronously and in queue,
        /// so full delivery may take a few minutes.</u></b>
        /// </remarks> 
        [Authorize(Roles = "Admin,ArtShow")]
        [HttpPost("AgentClosingResults/NotificationBundles/Send")]
        [ProducesResponseType(204)]
        public async Task<ActionResult> ExecuteAgentNotificationRunAsync()
        {
            await _agentClosingResultService.ExecuteNotificationRunAsync();
            return NoContent();
        }

        /// <summary>
        /// Delete unprocessed rows
        /// </summary>
        /// <remarks>
        /// Deletes any imported data for which no notification has been sent yet.<br /><br />
        /// <b>Use this to remove imported data if the simulation does not check out.</b>
        /// </remarks>
        [Authorize(Roles = "Admin,ArtShow")]
        [HttpDelete("AgentClosingResults/NotificationBundles/:unprocessed")]
        [ProducesResponseType(204)]
        public async Task<ActionResult> DeleteUnprocessedAgentClosingResultsImportRowsAsync()
        {
            await _agentClosingResultService.DeleteUnprocessedImportRowsAsync();
            return NoContent();
        }
    }
}