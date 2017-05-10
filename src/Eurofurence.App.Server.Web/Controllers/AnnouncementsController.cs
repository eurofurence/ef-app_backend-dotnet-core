using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Eurofurence.App.Domain.Model.Sync;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Web.Extensions;
using Eurofurence.App.Domain.Model.Announcements;
using Microsoft.AspNetCore.Authorization;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/v2/[controller]")]
    public class AnnouncementsController : Controller
    {
        readonly IAnnouncementService _announcementService;
        readonly IWnsChannelManager _wnsChannelManager;

        public AnnouncementsController(IAnnouncementService announcementService, IWnsChannelManager wnsChannelManager)
        {
            _wnsChannelManager = wnsChannelManager;
            _announcementService = announcementService;
        }

        /// <summary>
        /// Retrieves a list of all announcement entries.
        /// </summary>
        /// <returns>All Announcement Entries.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(IEnumerable<AnnouncementRecord>), 200)]
        public Task<IEnumerable<AnnouncementRecord>> GetAnnouncementEntriesAsync()
        {
            return _announcementService.FindAllAsync();
        }

        /// <summary>
        /// Retrieve a single announcement.
        /// </summary>
        /// <param name="id">id of the requested entity</param>
        [HttpGet("{Id}")]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(AnnouncementRecord), 200)]
        public async Task<AnnouncementRecord> GetAnnouncementAsync([FromRoute] Guid id)
        {
            return (await _announcementService.FindOneAsync(id)).Transient404(HttpContext);
        }

        

        [HttpDelete("{Id}")]
        [Authorize(Roles = "System,Developer")]
        public async Task<ActionResult> DeleteAnnouncementAsync([FromRoute] Guid id)
        {
            if (await _announcementService.FindOneAsync(id) == null) return NotFound();
            await _announcementService.DeleteOneAsync(id);

            _wnsChannelManager.PushSyncUpdateRequestAsync("Debug");

            return Ok();
        }


        [HttpPost]
        [Authorize(Roles = "System,Developer")]
        public async Task<ActionResult> PostAnnouncementAsync([FromBody] AnnouncementRecord record)
        {
            record.Touch();
            record.NewId();

            await _announcementService.InsertOneAsync(record);
            _wnsChannelManager.PushSyncUpdateRequestAsync("Debug");

            return Ok();
        }

        [HttpDelete]
        [Authorize(Roles = "System,Developer")]
        public async Task<ActionResult> ClearAnnouncementAsync()
        {
            await _announcementService.DeleteAllAsync();
            _wnsChannelManager.PushSyncUpdateRequestAsync("Debug");
            return Ok();
        }
    }
}
