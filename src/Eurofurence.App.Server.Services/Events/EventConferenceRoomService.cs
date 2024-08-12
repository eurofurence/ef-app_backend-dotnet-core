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
    public class EventConferenceRoomService : EntityServiceBase<EventConferenceRoomRecord>,
        IEventConferenceRoomService
    {
        private readonly AppDbContext _appDbContext;

        public EventConferenceRoomService(
            AppDbContext appDbContext,
            IStorageServiceFactory storageServiceFactory
        )
            : base(appDbContext, storageServiceFactory)
        {
            _appDbContext = appDbContext;
        }

        public override async Task<EventConferenceRoomRecord> FindOneAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _appDbContext.EventConferenceRooms
                .Include(eventConferenceRoom => eventConferenceRoom.Events)
                .AsNoTracking()
                .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        }

        public override IQueryable<EventConferenceRoomRecord> FindAll()
        {
            return _appDbContext.EventConferenceRooms
                .Include(eventConferenceRoom => eventConferenceRoom.Events)
                .AsNoTracking();
        }
    }
}