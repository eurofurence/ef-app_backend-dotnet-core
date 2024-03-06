using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Server.Services.Abstractions.Meetups;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Eurofurence.App.Domain.Model.Meetups;
using Eurofurence.App.Server.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Eurofurence.App.Domain.Model.Dealers;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/[controller]")]
    public class MeetupsController : BaseController
    {
        private readonly IMeetupService _meetupService;
        public MeetupsController (IMeetupService meetupService)
        {
            _meetupService = meetupService;
        }

        /// <summary>
        ///     Retrieves a list of all meetups.
        /// </summary>
        /// <returns>All meetups.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(IEnumerable<Meetup>), 200)]
        public Task<IEnumerable<Meetup>> GetMeetupEntriesAsync()
        {
            return _meetupService.FindAllAsync();
        }

        /// <summary>
        ///     Retrieve a single meetup.
        /// </summary>
        /// <param name="id">id of the requested entity</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(Meetup), 200)]
        public async Task<Meetup> GetMeetupAsync([FromRoute] Guid id)
        {
            return (await _meetupService.FindOneAsync(id)).Transient404(HttpContext);
        }

        /// <summary>
        ///     Delete a meetup.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [Authorize(Roles = "System,Developer")]
        public async Task<ActionResult> DeleteMeetupAsync([FromRoute] Guid id)
        {
            if (await _meetupService.FindOneAsync(id) == null) return NotFound();

            await _meetupService.DeleteOneAsync(id);

            return Ok();
        }

        /// <summary>
        ///     Create a new meetup.
        /// </summary>
        /// <param name="meetup"></param>
        /// <returns>Id of the newly created dealer</returns>
        [Authorize(Roles = "System,Developer")]
        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(400)]
        [HttpPost("")]
        public async Task<ActionResult> PostMeetupAsync(
            [EnsureNotNull][FromBody] Meetup meetup
        )
        {
            meetup.NewId();
            meetup.Touch();

            await _meetupService.InsertOneAsync(meetup);

            return Ok(meetup.Id);
        }

        /// <summary>
        ///     Update an existing meetup.
        /// </summary>
        /// <param name="meetup"></param>
        /// <param name="id"></param>
        [Authorize(Roles = "System,Developer")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 404)]
        [EnsureNotNull]
        [HttpPut("{id}")]
        public async Task<ActionResult> PutMeetupAsync(
            [EnsureNotNull][FromBody][EnsureEntityIdMatches("id")] Meetup meetup,
            [EnsureNotNull][FromRoute] Guid id)
        {
            var exists = await _meetupService.HasOneAsync(id);
            if (!exists) return NotFound($"No record found with it {id}");

            meetup.Touch();

            await _meetupService.ReplaceOneAsync(meetup);

            return NoContent();
        }
    }
}
