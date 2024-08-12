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
    public class EventConferenceTrackService : EntityServiceBase<EventConferenceTrackRecord>,
        IEventConferenceTrackService
    {
        private readonly AppDbContext _appDbContext;

        public EventConferenceTrackService(
            AppDbContext appDbContext,
            IStorageServiceFactory storageServiceFactory
        )
            : base(appDbContext, storageServiceFactory)
        {
            _appDbContext = appDbContext;
        }

        public override async Task<EventConferenceTrackRecord> FindOneAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _appDbContext.EventConferenceTracks
                .Include(eventConferenceTrack => eventConferenceTrack.Events)
                .AsNoTracking()
                .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        }

        public override IQueryable<EventConferenceTrackRecord> FindAll()
        {
            return _appDbContext.EventConferenceTracks
                .Include(eventConferenceTrack => eventConferenceTrack.Events)
                .AsNoTracking();
        }
    }
}