using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Domain.Model.Identity;
using Eurofurence.App.Domain.Model.Transformers;
using Eurofurence.App.Server.Services.Abstractions.Announcements;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;
using Eurofurence.App.Server.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/[controller]")]
    public class AnnouncementsController : BaseController
    {
        private readonly IAnnouncementService _announcementService;
        private readonly IImageService _imageService;
        private readonly IPushNotificationChannelManager _pushNotificationChannelManager;

        public AnnouncementsController(IAnnouncementService announcementService, IImageService imageService,
            IPushNotificationChannelManager pushNotificationChannelManager)
        {
            _announcementService = announcementService;
            _imageService = imageService;
            _pushNotificationChannelManager = pushNotificationChannelManager;
        }

        /// <summary>
        ///     Retrieves a list of all announcement entries.
        /// </summary>
        /// <remarks>
        /// The combination of Authorize and AllowAnonymous attributes is needed so Swagger correctly authorizes against the endpoint when a token is provided.
        /// It should not affect API behaviour as Authorize is ignored when AllowAnonymous is provided.
        /// This endpoint works without authentication.
        /// </remarks>
        /// <returns>All Announcement Entries.</returns>
        [Authorize]
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(IEnumerable<AnnouncementResponse>), 200)]
        public IEnumerable<AnnouncementResponse> GetAnnouncementEntries()
        {
            return _announcementService.FindAll().Select(x => x.Transform());
        }

        [Authorize(Roles = $"{IdentityRoles.Admin},{IdentityRoles.AnnouncementManager}")]
        [HttpGet(":all")]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(IEnumerable<AnnouncementResponse>), 200)]
        public IEnumerable<AnnouncementResponse> GetAllAnnouncements()
        {
            return _announcementService.FetchAll().Select(x => x.Transform());
        }

        /// <summary>
        ///     Retrieve a single announcement.
        /// </summary>
        /// <remarks>
        /// The combination of Authorize and AllowAnonymous attributes is needed so Swagger correctly authorizes against the endpoint when a token is provided.
        /// It should not affect API behaviour as Authorize is ignored when AllowAnonymous is provided.
        /// This endpoint works without authentication.
        /// </remarks>
        /// <param name="id">id of the requested entity</param>
        [Authorize]
        [AllowAnonymous]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(AnnouncementResponse), 200)]
        public async Task<AnnouncementResponse> GetAnnouncementAsync([FromRoute] Guid id)
        {
            return (await _announcementService.FindOneAsync(id)).Transient404(HttpContext)?.Transform();
        }

        /// <summary>
        /// Deletes a single announcement
        /// </summary>
        /// <param name="id">ID of the announcement to be deleted</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = $"{IdentityRoles.Admin},{IdentityRoles.AnnouncementManager}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<ActionResult> DeleteAnnouncementAsync([FromRoute] Guid id)
        {
            if (await QueryRecordForIdAsync(id) == null) return NotFound();

            await _announcementService.DeleteOneAsync(id);
            await _pushNotificationChannelManager.PushSyncRequestAsync();

            return NoContent();
        }


        /// <summary>
        /// Creates a new announcement and push it to all registered devices.
        /// </summary>
        /// <param name="request">New announcement to be pushed</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = $"{IdentityRoles.Admin},{IdentityRoles.AnnouncementManager}")]
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(string), 409)]
        public async Task<ActionResult> PostAnnouncementAsync([EnsureNotNull][FromBody] AnnouncementRequest request)
        {
            if (request.ImageId is Guid imageId && (await _imageService.FindOneAsync(imageId)) is null)
                return NotFound($"Unknown image ID {imageId}.");

            var record = request.Transform();
            await _announcementService.InsertOneAsync(record);
            await _pushNotificationChannelManager.PushSyncRequestAsync();

            if (request.Groups is { Length: > 0 })
            {
                await _pushNotificationChannelManager.PushAnnouncementNotificationToGroupsAsync(record, request.Groups);
            }
            else
            {
                await _pushNotificationChannelManager.PushAnnouncementNotificationAsync(record);
            }

            AnnouncementResponse resp = record.Transform();

            return Ok(record.Id);
        }

        /// <summary>
        /// Updates an existing announcement and requests all devices to sync their data.
        /// </summary>
        /// <param name="id">ID of existing announcement record</param>
        /// <param name="request">Updated announcement record</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize(Roles = $"{IdentityRoles.Admin},{IdentityRoles.AnnouncementManager}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<ActionResult> PutAnnouncementAsync([FromRoute] Guid id,
            [EnsureNotNull][FromBody] AnnouncementRequest request)
        {
            if (request == null)
            {
                return BadRequest("Error parsing request");
            }

            AnnouncementRecord record = await QueryRecordForIdAsync(id);

            if (record is not { } announcementRecord)
            {
                return NotFound();
            }

            announcementRecord.Merge(request);
            announcementRecord.Touch();

            await _announcementService.ReplaceOneAsync(announcementRecord);
            await _pushNotificationChannelManager.PushSyncRequestAsync();

            return NoContent();
        }

        /// <summary>
        /// Returns the announcement record for the given id.
        /// If the user is an admin or announcement manager, the record is returned regardless of whether
        /// they are a member of the groups or not.
        /// </summary>
        /// <param name="id">The id to look up.</param>
        /// <returns>Task with the record. Can be null.</returns>
        private async Task<AnnouncementRecord> QueryRecordForIdAsync(Guid id)
        {
            AnnouncementRecord record;

            if (User.IsInRole(IdentityRoles.Admin) ||
                User.IsInRole(IdentityRoles.AnnouncementManager))
            {
                record = await _announcementService.FindOneInAllAsync(id);
            }
            else
            {
                record = await _announcementService.FindOneAsync(id);
            }
            return record;
        }

        /// <summary>
        /// !DANGER! – Deletes all announcements from the database!
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        [Authorize(Roles = $"{IdentityRoles.Admin},{IdentityRoles.AnnouncementManager}")]
        [ProducesResponseType(204)]
        public async Task<ActionResult> ClearAnnouncementAsync()
        {
            await _announcementService.DeleteAllAsync();
            await _pushNotificationChannelManager.PushSyncRequestAsync();

            return NoContent();
        }
    }
}