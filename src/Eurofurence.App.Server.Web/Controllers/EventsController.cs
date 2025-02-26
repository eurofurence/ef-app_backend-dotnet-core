﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Server.Services.Abstractions.Events;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Eurofurence.App.Server.Services.Abstractions.PushNotifications;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Eurofurence.App.Server.Web.Extensions;
using Ical.Net;
using Ical.Net.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/[controller]")]
    public class
        EventsController : BaseController
    {
        private readonly IEventService _eventService;
        private readonly IImageService _imageService;
        private readonly IRegistrationIdentityService _registrationIdentityService;

        public EventsController(IEventService eventService,
            IImageService imageService, IRegistrationIdentityService registrationIdentityService)
        {
            _eventService = eventService;
            _imageService = imageService;
            _registrationIdentityService = registrationIdentityService;
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
            return _eventService.FindConflicts(conflictStartTime, conflictEndTime,
                TimeSpan.FromMinutes(toleranceInMinutes));
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
        /// Returns the favorite events of the currently logged-in user
        /// </summary>
        /// <returns>A list of <see cref="EventRecord"/> marked as favorite</returns>
        [Authorize]
        [HttpGet("Favorites")]
        [ProducesResponseType(200)]
        public Task<ActionResult> GetMyFavorites()
        {
            return Task.FromResult<ActionResult>(Ok(_eventService.GetFavoriteEventsFromUser(User)));
        }

        [Authorize]
        [HttpGet("Favorites/:calendar-token")]
        public async Task<ActionResult> GetFavoritesUrl()
        {
            var userId = User.GetSubject();

            var userToken = await _registrationIdentityService.GetOrCreateUserCalendarToken(User);

            if (userToken == null)
            {
                return BadRequest("User is unknown. This should technically never happen.");
            }

            return Ok(userToken);
        }

        [HttpGet("Favorites/calendar.ics/")]
        public async Task<ActionResult> FavoritesCalendar([FromQuery, BindRequired] string token)
        {
            if (token == null)
            {
                return Unauthorized("Token is required.");
            }

            if (await _registrationIdentityService.FindAll(record => record.CalendarToken == token)
                    .Include(record => record.FavoriteEvents)
                    .FirstOrDefaultAsync() is not { } userRecord)
            {
                return Unauthorized("Token is invalid.");
            }


            Calendar favoriteEvents = _eventService.GetFavoriteEventsFromUserAsIcal(userRecord);
            var serializer = new CalendarSerializer();
            var serializedCalendar = serializer.SerializeToString(favoriteEvents);

            return File(Encoding.ASCII.GetBytes(serializedCalendar), "text/calendar", "calendar.ics");
            //return Content(serializedCalendar, "text/iCal");
        }

        /// <summary>
        /// Adds an event to favorites
        /// </summary>
        /// <param name="id">The id of the event</param>
        /// <returns>Just a status code</returns>
        [Authorize]
        [HttpPost("{id}/:favorite")]
        public async Task<ActionResult> MarkEventAsFavorite([FromRoute] Guid id)
        {
            var foundEvent = await _eventService.FindOneAsync(id);

            if (foundEvent == null)
            {
                return NotFound();
            }

            await _eventService.AddEventToFavoritesIfNotExist(User, foundEvent);
            return NoContent();
        }

        /// <summary>
        /// Removes an event from favorites
        /// </summary>
        /// <param name="id">The id of the event</param>
        /// <returns>Just a status code</returns>
        [Authorize]
        [HttpDelete("{id}/:favorite")]
        public async Task<ActionResult> UnmarkEventAsFavorite([FromRoute] Guid id)
        {
            var events = _eventService.FindAll(x => x.Id == id);

            if (!events.Any())
            {
                return NotFound();
            }

            await _eventService.RemoveEventFromFavoritesIfExist(User, events.First());

            return NoContent();
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
        public async Task<ActionResult> PutEventBannerImageIdAsync([EnsureNotNull][FromBody] Guid? imageId,
            [FromRoute] Guid id)
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
        public async Task<ActionResult> PutEventPosterImageIdAsync([EnsureNotNull][FromBody] Guid? imageId,
            [FromRoute] Guid id)
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