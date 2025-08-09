using Eurofurence.App.Domain.Model.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.PushNotifications;
using Ical.Net;

namespace Eurofurence.App.Server.Services.Abstractions.Events
{
    public interface IEventService :
        IEntityServiceOperations<EventRecord, EventResponse>,
        IPatchOperationProcessor<EventRecord>
    {
        IQueryable<EventRecord> FindConflicts(
            DateTime conflictStartTime,
            DateTime conflictEndTime,
            TimeSpan tolerance,
            bool includeInternal);

        public Task RunImportAsync();

        /// <summary>
        /// Adds the given event to the users (<paramref name="user"/>) favorites
        /// </summary>
        /// <param name="user">The user to which the favorite should be added to</param>
        /// <param name="eventRecords">The event that should be marked as favorite</param>
        /// <returns></returns>
        Task AddEventToFavoritesIfNotExist([NotNull] ClaimsPrincipal user, EventRecord eventRecords);

        /// <summary>
        /// Removes a given event from the users (<paramref name="user"/>) favorites if it exists.
        /// </summary>
        /// <param name="user">The user from which the favorite should be removed from</param>
        /// <param name="eventRecord">The event that should be unmarked</param>
        /// <returns></returns>
        Task RemoveEventFromFavoritesIfExist([NotNull] ClaimsPrincipal user, EventRecord eventRecord);

        /// <summary>
        /// Returns all favorite events associated with that user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        List<EventRecord> GetFavoriteEventsFromUser([NotNull] ClaimsPrincipal user);

        /// <summary>
        /// Returns all favorite events of a user in an iCal Calendar format.
        /// </summary>
        /// <param name="user">The user whose events should be returned</param>
        /// <returns>A <see cref="Calendar"/> instance with all favorite events of the user </returns>
        Calendar GetFavoriteEventsFromUserAsIcal([NotNull] UserRecord user);
    }
}