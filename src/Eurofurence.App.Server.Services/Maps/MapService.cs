using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Eurofurence.App.Domain.Model.Maps;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Maps;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Eurofurence.App.Domain.Model.Sync;

namespace Eurofurence.App.Server.Services.Maps
{
    public class MapService : EntityServiceBase<MapRecord>, IMapService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IStorageService _storageService;

        public MapService(
            AppDbContext appDbContext,
            IStorageServiceFactory storageServiceFactory
        )
            : base(appDbContext, storageServiceFactory)
        {
            _appDbContext = appDbContext;
            _storageService = storageServiceFactory.CreateStorageService<MapRecord>();
        }

        public override async Task<MapRecord> FindOneAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _appDbContext.Maps
                .Include(m => m.Entries)
                .ThenInclude(me => me.Links)
                .AsNoTracking()
                .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        }

        public override IQueryable<MapRecord> FindAll()
        {
            return _appDbContext.Maps
                .Include(m => m.Entries)
                .ThenInclude(me => me.Links)
                .AsNoTracking();
        }

        public override async Task<DeltaResponse<MapRecord>> GetDeltaResponseAsync(
            DateTime? minLastDateTimeChangedUtc = null,
            CancellationToken cancellationToken = default)
        {
            var storageInfo = await GetStorageInfoAsync(cancellationToken);
            var response = new DeltaResponse<MapRecord>
            {
                StorageDeltaStartChangeDateTimeUtc = storageInfo.DeltaStartDateTimeUtc,
                StorageLastChangeDateTimeUtc = storageInfo.LastChangeDateTimeUtc
            };

            if (!minLastDateTimeChangedUtc.HasValue || minLastDateTimeChangedUtc < storageInfo.DeltaStartDateTimeUtc)
            {
                response.RemoveAllBeforeInsert = true;
                response.DeletedEntities = Array.Empty<Guid>();
                response.ChangedEntities = await
                    _appDbContext.Maps
                        .Include(d => d.Entries)
                        .ThenInclude(me => me.Links)
                        .Where(entity => entity.IsDeleted == 0)
                        .ToArrayAsync(cancellationToken);
            }
            else
            {
                response.RemoveAllBeforeInsert = false;

                var entities = _appDbContext.Maps
                    .Include(d => d.Entries)
                    .ThenInclude(me => me.Links)
                    .IgnoreQueryFilters()
                    .Where(entity => entity.LastChangeDateTimeUtc > minLastDateTimeChangedUtc);

                response.ChangedEntities = await entities
                    .Where(a => a.IsDeleted == 0)
                    .ToArrayAsync(cancellationToken);
                response.DeletedEntities = await entities
                    .Where(a => a.IsDeleted == 1)
                    .Select(a => a.Id)
                    .ToArrayAsync(cancellationToken);
            }

            return response;
        }

        public override async Task InsertOneAsync(MapRecord entity, CancellationToken cancellationToken = default)
        {
            var entriesToAdd = entity.Entries;

            entity.Entries = new List<MapEntryRecord>();
            await base.InsertOneAsync(entity, cancellationToken);

            foreach (var entry in entriesToAdd)
            {
                if (_appDbContext.MapEntries.Any(mapEntry => mapEntry.Id == entry.Id))
                {
                    entry.MapId = entity.Id;
                    await ReplaceOneEntryAsync(entry, cancellationToken);
                    continue;
                }

                entry.MapId = entity.Id;
                await InsertOneEntryAsync(entry, cancellationToken);
            }
        }

        public override async Task ReplaceOneAsync(MapRecord entity, CancellationToken cancellationToken = default)
        {
            var existingEntity = await FindOneAsync(entity.Id, cancellationToken);

            foreach (var existingEntry in existingEntity.Entries)
            {
                if (entity.Entries.All(mapEntry => mapEntry.Id != existingEntry.Id))
                {
                    await DeleteOneEntryAsync(existingEntry.Id, cancellationToken);
                }
            }

            foreach (var entry in entity.Entries)
            {
                if (_appDbContext.MapEntries.Any(mapEntry => mapEntry.Id == entry.Id))
                {
                    entry.MapId = entity.Id;
                    await ReplaceOneEntryAsync(entry, cancellationToken);
                    continue;
                }

                entry.MapId = entity.Id;
                await InsertOneEntryAsync(entry, cancellationToken);
            }
            await base.ReplaceOneAsync(entity, cancellationToken);
        }

        public async Task InsertOneEntryAsync(MapEntryRecord entity, CancellationToken cancellationToken = default)
        {
            var map = await _appDbContext.Maps
                .FirstOrDefaultAsync(map => map.Id == entity.MapId, cancellationToken);
            _appDbContext.MapEntries.Add(entity);
            await _appDbContext.SaveChangesAsync(cancellationToken);
            await _storageService.TouchAsync(cancellationToken);
        }

        public async Task ReplaceOneEntryAsync(MapEntryRecord entity, CancellationToken cancellationToken = default)
        {
            var existingEntity = await _appDbContext.MapEntries
                .Include(mapEntryRecord => mapEntryRecord.Links)
                .AsNoTracking()
                .FirstOrDefaultAsync(me => me.Id == entity.Id, cancellationToken);

            foreach (var existingLink in existingEntity.Links)
            {
                var entityLinkInNewEntity = entity.Links.FirstOrDefault(link => Equals(link, existingLink));

                if (entityLinkInNewEntity != null)
                {
                    entityLinkInNewEntity.Id = existingLink.Id;
                }
                else
                {
                    _appDbContext.LinkFragments.Remove(existingLink);
                }
            }

            foreach (var link in entity.Links)
            {
                if (!existingEntity.Links.Contains(link))
                {
                    _appDbContext.LinkFragments.Add(link);
                }
            }

            _appDbContext.MapEntries.Update(entity);
            await _appDbContext.SaveChangesAsync(cancellationToken);
            await _storageService.TouchAsync(cancellationToken);
        }

        public async Task DeleteOneEntryAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _appDbContext.MapEntries
                .Include(me => me.Links)
                .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
            _appDbContext.Remove(entity);
            await _storageService.TouchAsync(cancellationToken);
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAllEntriesAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entities = _appDbContext.MapEntries
                .Include(me => me.Links)
                .Where(entity => entity.MapId == id);

            _appDbContext.RemoveRange(entities);

            await _storageService.TouchAsync(cancellationToken);
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}