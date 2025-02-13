using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Domain.Model.PushNotifications;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Events;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Microsoft.EntityFrameworkCore;

namespace Eurofurence.App.Server.Services.Events
{
    public class EventFavoritesService(
        AppDbContext appDbContext,
        IStorageServiceFactory storageServiceFactory,
        bool useSoftDelete = true)
        : IEventFavoritesService
    {
        public async Task AddEventToFavoritesIfNotExist(ClaimsPrincipal user, EventRecord eventRecords)
        {
            if (!appDbContext.Users.Any(x =>
                    x.IdentityId == user.GetSubject() && x.FavoriteEvents.Contains(eventRecords)))
            {
                UserRecord userRecord = appDbContext.Users
                    .Include(userRecord => userRecord.FavoriteEvents)
                    .First(x => x.IdentityId == user.GetSubject());

                if (!userRecord.FavoriteEvents.Contains(eventRecords))
                {
                    userRecord.FavoriteEvents.Add(eventRecords);
                }

                await appDbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Returns a list with all favorite events of a given user by their regid : <paramref name="user"/>
        /// </summary>
        /// <param name="user">The user whose events should be fetched</param>
        /// <returns>A list of all the events of the user</returns>
        public List<EventRecord> GetFavoriteEventsFromUser(ClaimsPrincipal user)
        {
            return appDbContext.Users
                .AsNoTracking()
                .Include(x => x.FavoriteEvents)
                .Where(x => x.IdentityId == user.GetSubject())
                .Select(x => x.FavoriteEvents).First();
        }

        public async Task RemoveEventFromFavoritesIfExist(ClaimsPrincipal user, EventRecord eventRecord)
        {
            var foundRecord = appDbContext.Users
                .Include(x => x.FavoriteEvents)
                .First(x => x.IdentityId == user.GetSubject());

            if (foundRecord.FavoriteEvents.FirstOrDefault(x => x.Id == eventRecord.Id) is { } eventRecordToRemove)
            {
                foundRecord.FavoriteEvents.Remove(eventRecordToRemove);
            }

            await appDbContext.SaveChangesAsync();
        }
    }
}