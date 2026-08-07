using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Domain.Model.Users;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Events;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Events
{
    public class EventFavoriteStatisticsService : EntityServiceBase<EventFavoriteStatisticsRecord, EventFavoriteStatisticsResponse>,
        IEventFavoriteStatisticsService
    {
        private readonly AppDbContext _appDbContext;

        public EventFavoriteStatisticsService(
            AppDbContext appDbContext,
            IStorageServiceFactory storageServiceFactory
        )
            : base(appDbContext, storageServiceFactory)
        {
            _appDbContext = appDbContext;
        }

        /// <inheritdoc/>
        public async Task InsertForAllStartedEvents()
        {
            var eventsWithoutFavoriteStatistics = await _appDbContext.Events
                .AsNoTracking()
                .Include(e => e.FavoredBy)
                .Include(e => e.FavoriteStatistics)
                .Where(e => e.FavoriteStatistics.Count == 0 && e.StartDateTimeUtc <= DateTime.UtcNow)
                .ToListAsync();

            foreach (var eventWithoutFavoriteStatistics in eventsWithoutFavoriteStatistics)
            {
                await InsertMultipleAsync(ComputeEventFavoriteStatistics(eventWithoutFavoriteStatistics));
            }
        }

        /// <inheritdoc/>
        public List<EventFavoriteStatisticsRecord> ComputeEventFavoriteStatistics(EventRecord eventRecord)
        {
            var result = from registrationStatus in Enum.GetValues<UserRegistrationStatus>()
                         select new EventFavoriteStatisticsRecord
                         {
                             EventId = eventRecord.Id,
                             UserRegistrationStatus = registrationStatus,
                             Count = eventRecord.FavoredBy
                                 .GroupBy(x => x.IdentityId).Select(y => y.FirstOrDefault())
                                 .Count(e => e.RegistrationStatus == registrationStatus)
                         };
            return result.ToList();
        }
    }
}
