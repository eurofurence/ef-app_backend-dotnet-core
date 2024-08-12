using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Events;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Threading;
using Microsoft.EntityFrameworkCore;

namespace Eurofurence.App.Server.Services.Events
{
    public class EventConferenceDayService : EntityServiceBase<EventConferenceDayRecord>,
        IEventConferenceDayService
    {
        private readonly AppDbContext _appDbContext;

        public EventConferenceDayService(
            AppDbContext appDbContext,
            IStorageServiceFactory storageServiceFactory
        )
            : base(appDbContext, storageServiceFactory)
        {
            _appDbContext = appDbContext;
        }

        public override async Task<EventConferenceDayRecord> FindOneAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _appDbContext.EventConferenceDays
                .Include(eventConferenceDay => eventConferenceDay.Events)
                .AsNoTracking()
                .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        }

        public override IQueryable<EventConferenceDayRecord> FindAll()
        {
            return _appDbContext.EventConferenceDays
                .Include(eventConferenceDay => eventConferenceDay.Events)
                .AsNoTracking();
        }
    }
}