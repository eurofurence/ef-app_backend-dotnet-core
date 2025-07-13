using Eurofurence.App.Domain.Model.Events;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Events;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Threading;
using Eurofurence.App.Server.Services.Abstractions.Maps;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Eurofurence.App.Domain.Model.Sync;
using Eurofurence.App.Server.Web.Controllers.Transformers;

namespace Eurofurence.App.Server.Services.Events
{
    public class EventConferenceRoomService : EntityServiceBase<EventConferenceRoomRecord, EventConferenceRoomResponse>,
        IEventConferenceRoomService
    {
        private readonly AppDbContext _appDbContext;
        private readonly MapOptions _mapOptions;

        public EventConferenceRoomService(
            AppDbContext appDbContext,
            IStorageServiceFactory storageServiceFactory,
            IOptions<MapOptions> mapOptions)
            : base(appDbContext, storageServiceFactory)
        {
            _appDbContext = appDbContext;
            _mapOptions = mapOptions.Value;
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

        public string GetMapLink(Guid id)
        {
            return _mapOptions.UrlTemplate.Replace("{id}", id.ToString());
        }

        public override async Task<DeltaResponse<EventConferenceRoomResponse>> GetDeltaResponseAsync(
            DateTime? minLastDateTimeChangedUtc = null,
            CancellationToken cancellationToken = default)
        {
            var storageInfo = await GetStorageInfoAsync(cancellationToken);
            var response = new DeltaResponse<EventConferenceRoomResponse>
            {
                StorageDeltaStartChangeDateTimeUtc = storageInfo.DeltaStartDateTimeUtc,
                StorageLastChangeDateTimeUtc = storageInfo.LastChangeDateTimeUtc
            };

            if (!minLastDateTimeChangedUtc.HasValue || minLastDateTimeChangedUtc < storageInfo.DeltaStartDateTimeUtc)
            {
                response.RemoveAllBeforeInsert = true;
                response.DeletedEntities = Array.Empty<Guid>();

                var changedEntities = await
                    _appDbContext.EventConferenceRooms
                        .Where(entity => entity.IsDeleted == 0)
                        .Select(x => x.Transform())
                        .ToListAsync(cancellationToken);

                changedEntities.ForEach(x => x.MapLink = GetMapLink(x.Id));

                response.ChangedEntities = changedEntities
                        .ToArray();
            }
            else
            {
                response.RemoveAllBeforeInsert = false;

                var entities = _appDbContext.EventConferenceRooms.IgnoreQueryFilters()
                    .Where(entity => entity.LastChangeDateTimeUtc > minLastDateTimeChangedUtc);

                var changedEntities = await entities
                    .Where(a => a.IsDeleted == 0)
                    .Select(x => x.Transform())
                    .ToListAsync(cancellationToken);

                changedEntities.ForEach(x => x.MapLink = GetMapLink(x.Id));

                response.ChangedEntities = changedEntities
                    .ToArray();

                response.DeletedEntities = await entities
                    .Where(a => a.IsDeleted == 1)
                    .Select(a => a.Id)
                    .ToArrayAsync(cancellationToken);
            }

            return response;
        }
    }
}