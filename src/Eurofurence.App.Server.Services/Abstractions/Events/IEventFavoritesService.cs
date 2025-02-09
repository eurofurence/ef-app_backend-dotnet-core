using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Events;

namespace Eurofurence.App.Server.Services.Abstractions.Events;

/// <summary>
/// Service for managing event favorites
/// </summary>
public interface IEventFavoritesService : IEntityServiceOperations<EventFavoriteRecord>
{
    /// <summary>
    /// Adds the given event to the users (<paramref name="user"/>) favorites
    /// </summary>
    /// <param name="user">The user to which the favorite should be added to</param>
    /// <param name="eventRecords">The event that should be marked as favorite</param>
    /// <returns></returns>
    Task AddEventToFavoritesIfNotExist(ClaimsPrincipal user, EventRecord eventRecords);

    /// <summary>
    /// Removes a given event from the users (<paramref name="user"/>) favorites if it exists.
    /// </summary>
    /// <param name="user">The user from which the favorite should be removed from</param>
    /// <param name="eventRecord">The event that should be unmarked</param>
    /// <returns></returns>
    Task RemoveEventFromFavoritesIfExist(ClaimsPrincipal user, EventRecord eventRecord);

    /// <summary>
    /// Returns all favorite events associated with that user
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    List<EventRecord> GetFavoriteEventsFromUser(ClaimsPrincipal user);

}