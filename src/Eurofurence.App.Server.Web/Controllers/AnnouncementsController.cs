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
        private readonly IPushEventMediator _eventMediator;

        public AnnouncementsController(IAnnouncementService announcementService, IPushEventMediator eventMediator)
        {
            _announcementService = announcementService;
            _eventMediator = eventMediator;
        }

        /// <summary>
        ///     Retrieves a list of all announcement entries.
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
        ///     Retrieve a single announcement.
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
            await _eventMediator.PushSyncRequestAsync();

            return Ok();
        }


        [HttpPost]
        [Authorize(Roles = "System,Developer")]
        public async Task<ActionResult> PostAnnouncementAsync([FromBody] AnnouncementRecord record)
        {
            record.Touch();
            record.NewId();

            await _announcementService.InsertOneAsync(record);
            await _eventMediator.PushSyncRequestAsync();
            await _eventMediator.PushAnnouncementNotificationAsync(record);

            return Ok();
        }

        [HttpPut]
        [Authorize(Roles = "System,Developer")]
        public async Task<ActionResult> PutAnnouncementAsync([FromBody] AnnouncementRecord record)
        {
            if (record == null) return BadRequest("Error parsing Record");
            if (record.Id == Guid.Empty) return BadRequest("Error parsing Record.Id");

            var existingRecord = await _announcementService.FindOneAsync(record.Id);
            if (existingRecord == null) return NotFound($"No record found with it {record.Id}");

            record.Touch();

            await _announcementService.ReplaceOneAsync(record);
            await _eventMediator.PushSyncRequestAsync();

            return NoContent();
        }

        [HttpDelete]
        [Authorize(Roles = "System,Developer")]
        public async Task<ActionResult> ClearAnnouncementAsync()
        {
            await _announcementService.DeleteAllAsync();
            await _eventMediator.PushSyncRequestAsync();

            return Ok();
        }
    }
}