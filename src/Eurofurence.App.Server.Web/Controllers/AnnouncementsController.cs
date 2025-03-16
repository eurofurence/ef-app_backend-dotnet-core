﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Server.Services.Abstractions.Announcements;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;
using Eurofurence.App.Server.Web.Controllers.Transformers;
using Eurofurence.App.Server.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        /// <returns>All Announcement Entries.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(IEnumerable<AnnouncementRecord>), 200)]
        public IEnumerable<AnnouncementResponse> GetAnnouncementEntries()
        {
            return _announcementService.FindAll().Select(x => x.Transform());
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

        /// <summary>
        /// Deletes a single announcement
        /// </summary>
        /// <param name="id">ID of the announcement to be deleted</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<ActionResult> DeleteAnnouncementAsync([FromRoute] Guid id)
        {
            if (await _announcementService.FindOneAsync(id) == null) return NotFound();

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
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(string), 409)]
        public async Task<ActionResult> PostAnnouncementAsync([EnsureNotNull][FromBody] AnnouncementRequest request)
        {
            if (request.ImageId is Guid imageId && (await _imageService.FindOneAsync(imageId)) is null)
                return NotFound($"Unknown image ID {imageId}.");

            var record = request.Transform();
            await _announcementService.InsertOneAsync(record);
            await _pushNotificationChannelManager.PushSyncRequestAsync();
            await _pushNotificationChannelManager.PushAnnouncementNotificationAsync(record);

            AnnouncementResponse resp = record.Transform();

            return Ok(record.Id);
        }
        interface IA
        {
            void M() { WriteLine("IA.M"); }
            void WriteLine(string iaM);
        }
        class C : IA
        {
            public void WriteLine(string iaM)
            {
                throw new NotImplementedException();
            }
        } // OK
        /// <summary>
        /// Updates and existing announcement and requests all devices to sync their data.
        /// </summary>
        /// <param name="record">Updated announcement record</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 404)]
        public async Task<ActionResult> PutAnnouncementAsync([FromRoute] Guid id,
            [EnsureNotNull][FromBody] AnnouncementRequest request)
        {



            IA i = new C();
            i.M(); // prints "IA.M"

            if (await _announcementService.FindOneAsync(id) is not { } announcementRecord)
            {
                return NotFound();
            }
            announcementRecord.Merge(request);
            // if (record == null) return BadRequest("Error parsing Record");
            // if (record.Id == Guid.Empty) return BadRequest("Error parsing Record.Id");
            //
            // var existingRecord = await _announcementService.FindOneAsync(record.Id);
            // if (existingRecord == null) return NotFound($"No record found with it {record.Id}");

            announcementRecord.Touch();

            await _announcementService.ReplaceOneAsync(announcementRecord);
            await _pushNotificationChannelManager.PushSyncRequestAsync();

            return NoContent();
        }

        /// <summary>
        /// !DANGER! – Deletes all announcements from the database!
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(204)]
        public async Task<ActionResult> ClearAnnouncementAsync()
        {
            await _announcementService.DeleteAllAsync();
            await _pushNotificationChannelManager.PushSyncRequestAsync();

            return NoContent();
        }
    }
}