using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Server.Services.Abstractions.Announcements;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;
using Eurofurence.App.Server.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/[controller]")]
    public class AnnouncementsController : BaseController
    {
        private readonly IAnnouncementService _announcementService;
        private readonly IFirebaseChannelManager _firebaseChannelManager;

        public AnnouncementsController(IAnnouncementService announcementService, IFirebaseChannelManager firebaseChannelManager)
        {
            _announcementService = announcementService;
            _firebaseChannelManager = firebaseChannelManager;
        }

        /// <summary>
        ///     Retrieves a list of all announcement entries.
        /// </summary>
        /// <returns>All Announcement Entries.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(IEnumerable<AnnouncementRecord>), 200)]
        public IEnumerable<AnnouncementRecord> GetAnnouncementEntries()
        {
            return _announcementService.FindAll();
        }

        /// <summary>
        ///     Retrieve a single announcement.
        /// </summary>
        /// <param name="id">id of the requested entity</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(AnnouncementRecord), 200)]
        public async Task<AnnouncementRecord> GetAnnouncementAsync([FromRoute] Guid id)
        {
            return (await _announcementService.FindOneAsync(id)).Transient404(HttpContext);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteAnnouncementAsync([FromRoute] Guid id)
        {
            if (await _announcementService.FindOneAsync(id) == null) return NotFound();

            await _announcementService.DeleteOneAsync(id);
            await _firebaseChannelManager.PushSyncRequestAsync();

            return Ok();
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> PostAnnouncementAsync([FromBody] AnnouncementRecord record)
        {
            record.Touch();
            record.NewId();

            await _announcementService.InsertOneAsync(record);
            await _firebaseChannelManager.PushSyncRequestAsync();
            await _firebaseChannelManager.PushAnnouncementNotificationAsync(record);

            return Ok();
        }

        [HttpPut]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> PutAnnouncementAsync([FromBody] AnnouncementRecord record)
        {
            if (record == null) return BadRequest("Error parsing Record");
            if (record.Id == Guid.Empty) return BadRequest("Error parsing Record.Id");

            var existingRecord = await _announcementService.FindOneAsync(record.Id);
            if (existingRecord == null) return NotFound($"No record found with it {record.Id}");

            record.Touch();

            await _announcementService.ReplaceOneAsync(record);
            await _firebaseChannelManager.PushSyncRequestAsync();

            return NoContent();
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ClearAnnouncementAsync()
        {
            await _announcementService.DeleteAllAsync();
            await _firebaseChannelManager.PushSyncRequestAsync();

            return Ok();
        }
    }
}