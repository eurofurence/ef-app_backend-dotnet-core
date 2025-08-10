using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Domain.Model.Transformers;
using Eurofurence.App.Server.Services.Abstractions.Events;
using Eurofurence.App.Server.Web.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Eurofurence.App.Server.Web.Controllers
{
    [Route("Api/[controller]")]
    public class EventConferenceTracksController : BaseController
    {
        private readonly IEventConferenceTrackService _eventConferenceTrackService;

        public EventConferenceTracksController(IEventConferenceTrackService eventConferenceTrackService)
        {
            _eventConferenceTrackService = eventConferenceTrackService;
        }

        /// <summary>
        ///     Retrieves a list of all event conference tracks in the event schedule.
        /// </summary>
        /// <remarks>
        /// The combination of Authorize and AllowAnonymous attributes is needed so Swagger correctly authorizes against the endpoint when a token is provided.
        /// It should not affect API behaviour as Authorize is ignored when AllowAnonymous is provided.
        /// This endpoint works without authentication.
        /// </remarks>
        /// <returns>All events in the event schedule.</returns>
        [Authorize]
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(typeof(string), 404)]
        [ProducesResponseType(typeof(IEnumerable<EventConferenceTrackResponse>), 200)]
        public IQueryable<EventConferenceTrackResponse> GetEventConferenceTracksAsync()
        {
            var isStaff = User?.IsInRole("Staff") ?? false;
            return _eventConferenceTrackService.FindAll(x => isStaff || !x.IsInternal).Select(x => x.Transform());
        }

        /// <summary>
        ///     Retrieve a single event conference track in the event schedule.
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
        [ProducesResponseType(typeof(EventConferenceTrackResponse), 200)]
        public async Task<EventConferenceTrackResponse> GetEventConferenceTrackAsync([FromRoute] Guid id)
        {
            var isStaff = User?.IsInRole("Staff") ?? false;
            return (await _eventConferenceTrackService.FindAll(x => x.Id == id && (isStaff || !x.IsInternal)).FirstOrDefaultAsync()).Transient404(HttpContext)?.Transform();
        }
    }
}