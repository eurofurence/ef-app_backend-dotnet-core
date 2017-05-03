using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Eurofurence.App.Domain.Model.Sync;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Web.Extensions;
using Eurofurence.App.Domain.Model.Announcements;
using Microsoft.AspNetCore.Authorization;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/v2/[controller]")]
    public class AnnouncementsController : Controller
    {
        readonly IAnnouncementService _announcementService;

        public AnnouncementsController(IAnnouncementService announcementService)
        {
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

        /// <summary>
        /// Retrieves a delta of announcement entries since a given timestamp.
        /// </summary>
        /// <param name="since" type="query">Delta reference, date time in ISO 8610. If set, only items with a 
        /// LastChangeDateTimeUtc >= the specified value will be returned. If not set, API will return the current set 
        /// of records without deleted items. If set, items deleted since the delta specified will be returned with an 
        /// IsDeleted flag set.</param>
        [HttpGet("Delta")]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(DeltaResponse<AnnouncementRecord>), 200)]
        public Task<DeltaResponse<AnnouncementRecord>> GetAnnouncementEntriesDeltaAsync([FromQuery] DateTime? since = null)
        {
            return _announcementService.GetDeltaResponseAsync(minLastDateTimeChangedUtc: since);
        }

        [HttpDelete("{Id}")]
        [Authorize]
        public async Task<ActionResult> DeleteAnnouncementAsync([FromRoute] Guid id)
        {
            if (await _announcementService.FindOneAsync(id) == null) return NotFound();
            await _announcementService.DeleteOneAsync(id);

            return Ok();
        }
    }
}
