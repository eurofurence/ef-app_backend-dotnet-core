using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Events;
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
        : EntityServiceBase<EventFavoriteRecord>(appDbContext, storageServiceFactory, useSoftDelete),
            IEventFavoritesService
    {


        public async Task AddEventToFavoritesIfNotExist(ClaimsPrincipal user, EventRecord eventRecords)
        {
            if (!appDbContext.EventFavorites.Any(x => x.UserUid == user.GetSubject() && x.Event == eventRecords))
            {
                EventFavoriteRecord record = new EventFavoriteRecord
                {
                    UserUid = user.GetSubject(),
                    EventId = eventRecords.Id
                };
                appDbContext.EventFavorites.Add(record);
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
            return appDbContext.EventFavorites
                .AsNoTracking()
                .Where(x => x.UserUid == user.GetSubject())
                .Select(x => x.Event)
                .ToList();
        }

        public async Task RemoveEventFromFavoritesIfExist(ClaimsPrincipal user, EventRecord eventRecord)
        {
            var foundRecord = appDbContext.EventFavorites.FirstOrDefault(x => x.UserUid == user.GetSubject() && x.Event == eventRecord);
            if (foundRecord != null)
            {
                await base.DeleteOneAsync(foundRecord.Id);
            }
        }
    }
}