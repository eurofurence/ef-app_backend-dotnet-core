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
    public class EventFavoritesService : EntityServiceBase<EventFavoriteRecord>, IEventFavoritesService
    {

        private readonly AppDbContext _appDbContext;


        public EventFavoritesService(AppDbContext appDbContext, IStorageServiceFactory storageServiceFactory, bool useSoftDelete = true) : base(appDbContext, storageServiceFactory, useSoftDelete)
        {
            this._appDbContext = appDbContext;
        }

        public async Task AddEventToFavoritesIfNotExist(ClaimsPrincipal user, EventRecord eventRecords)
        {
            if (!_appDbContext.EventFavorites.Any(x => x.UserUid == user.GetSubject() && x.Event == eventRecords))
            {
                EventFavoriteRecord record = new EventFavoriteRecord(eventRecords, user.GetSubject());
                await _appDbContext.AddAsync(record);
            }
        }

        public List<EventRecord> GetFavoriteEventsFromUser(ClaimsPrincipal user)
        {
            return _appDbContext.EventFavorites
                .AsNoTracking()
                .Where(x => x.UserUid == user.GetSubject())
                .Select(x => x.Event)
                .ToList();
        }

        public async Task RemoveEventFromFavoritesIfExist(ClaimsPrincipal user, EventRecord eventRecord)
        {
            var foundRecord = _appDbContext.EventFavorites.FirstOrDefault(x => x.UserUid == user.GetSubject() && x.Event == eventRecord);
            if (foundRecord != null)
            {
                await base.DeleteOneAsync(foundRecord.Id);
            }
        }
    }
}