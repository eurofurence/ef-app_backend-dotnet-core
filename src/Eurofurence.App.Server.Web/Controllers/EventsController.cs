using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Server.Services.Abstractions.Events;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Server.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/[controller]")]
    public class EventsController : BaseController
    {
        private readonly IEventService _eventService;
        private readonly IImageService _imageService;

        public EventsController(IEventService eventService,
            IImageService imageService)
        {
            _eventService = eventService;
            _imageService = imageService;
        }

        /// <summary>
        ///     Retrieves a list of all events in the event schedule.
        /// </summary>
        /// <returns>All events in the event schedule.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(IEnumerable<EventRecord>), 200)]
        public IQueryable<EventRecord> GetEventsAsync()
        {
            return _eventService.FindAll();
        }

        /// <summary>
        ///     Retrieves a list of all events in the event schedule that
        ///     conflict with the specified start/end time, +/- a tolerance
        ///     in minutes that is considered when calculating overlaps.
        /// </summary>
        /// <returns>All events in the event schedule that conflict with a specified start/endtime + tolerance.</returns>
        [HttpGet(":conflicts")]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(IEnumerable<EventRecord>), 200)]
        public IQueryable<EventRecord> GetConflictingEventsAsync(
            DateTime conflictStartTime,
            DateTime conflictEndTime,
            int toleranceInMinutes
            )
        {
            return _eventService.FindConflicts(conflictStartTime, conflictEndTime, TimeSpan.FromMinutes(toleranceInMinutes));
        }

        /// <summary>
        ///     Retrieve a single event in the event schedule.
        /// </summary>
        /// <param name="id">id of the requested entity</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(EventRecord), 200)]
        public async Task<EventRecord> GetEventAsync([FromRoute] Guid id)
        {
            return (await _eventService.FindOneAsync(id)).Transient404(HttpContext);
        }

        /// <summary>
        ///     Update the banner image of an existing event in the event schedule.
        /// </summary>
        /// <param name="imageId">id of the image to be used</param>
        /// <param name="id">id of the event entity</param>
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 404)]
        [EnsureNotNull]
        [HttpPut("{id}/:bannerImageId")]
        public async Task<ActionResult> PutEventBannerImageIdAsync([EnsureNotNull][FromBody] Guid? imageId, [FromRoute] Guid id)
        {
            var eventRecord = await _eventService.FindOneAsync(id);
            if (eventRecord == null)
                return NotFound("Unknown event ID.");

            if (imageId.HasValue && !await _imageService.HasOneAsync(imageId.Value))
            {
                return NotFound("Unknown image ID.");
            }

            eventRecord.BannerImageId = imageId;
            await _eventService.ReplaceOneAsync(eventRecord);

            return NoContent();
        }

        /// <summary>
        ///     Update the poster image of an existing event in the event schedule.
        /// </summary>
        /// <param name="imageId">id of the image to be used</param>
        /// <param name="id">id of the event entity</param>
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 404)]
        [EnsureNotNull]
        [HttpPut("{id}/:posterImageId")]
        public async Task<ActionResult> PutEventPosterImageIdAsync([EnsureNotNull][FromBody] Guid? imageId, [FromRoute] Guid id)
        {
            var eventRecord = await _eventService.FindOneAsync(id);
            if (eventRecord == null)
                return NotFound("Unknown event ID.");

            if (imageId.HasValue && !await _imageService.HasOneAsync(imageId.Value))
            {
                return NotFound("Unknown image ID.");
            }

            eventRecord.PosterImageId = imageId;
            await _eventService.ReplaceOneAsync(eventRecord);

            return NoContent();
        }
    }
}