using System;
using System.Collections.Generic;
using System.Linq;
using Eurofurence.App.Domain.Model.Maps;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Maps;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Eurofurence.App.Server.Services.Maps
{
    public class MapService : EntityServiceBase<MapRecord>,
        IMapService
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

        public override async Task<MapRecord> FindOneAsync(Guid id)
        {
            return await _appDbContext.Maps
                .Include(m => m.Image)
                .Include(m => m.Entries)
                .ThenInclude(me => me.Links)
                .AsNoTracking()
                .FirstOrDefaultAsync(entity => entity.Id == id);
        }

        public override IQueryable<MapRecord> FindAll()
        {
            return _appDbContext.Maps
                .Include(m => m.Image)
                .Include(m => m.Entries)
                .ThenInclude(me => me.Links)
                .AsNoTracking();
        }

        public override async Task InsertOneAsync(MapRecord entity)
        {
            var entriesToAdd = entity.Entries;

            entity.Entries = new List<MapEntryRecord>();
            await base.InsertOneAsync(entity);

            foreach (var entry in entriesToAdd)
            {
                if (_appDbContext.MapEntries.Any(mapEntry => mapEntry.Id == entry.Id))
                {
                    entry.MapId = entity.Id;
                    await ReplaceOneEntryAsync(entry);
                    continue;
                }

                entry.MapId = entity.Id;
                await InsertOneEntryAsync(entry);
            }
        }

        public override async Task ReplaceOneAsync(MapRecord entity)
        {
            var existingEntity = await FindOneAsync(entity.Id);

            foreach (var existingEntry in existingEntity.Entries)
            {
                if (entity.Entries.All(mapEntry => mapEntry.Id != existingEntry.Id))
                {
                    await DeleteOneEntryAsync(existingEntry.Id);
                }
            }

            foreach (var entry in entity.Entries)
            {
                if (_appDbContext.MapEntries.Any(mapEntry => mapEntry.Id == entry.Id))
                {
                    entry.MapId = entity.Id;
                    await ReplaceOneEntryAsync(entry);
                    continue;
                }

                entry.MapId = entity.Id;
                await InsertOneEntryAsync(entry);
            }
            await base.ReplaceOneAsync(entity);
        }

        public async Task InsertOneEntryAsync(MapEntryRecord entity)
        {
            var map = await _appDbContext.Maps
                .FirstOrDefaultAsync(map => map.Id == entity.MapId);
            _appDbContext.MapEntries.Add(entity);
            await _appDbContext.SaveChangesAsync();
            await _storageService.TouchAsync();
        }

        public async Task ReplaceOneEntryAsync(MapEntryRecord entity)
        {
            var existingEntity = await _appDbContext.MapEntries
                .Include(mapEntryRecord => mapEntryRecord.Links)
                .AsNoTracking()
                .FirstOrDefaultAsync(me => me.Id == entity.Id);

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
            await _appDbContext.SaveChangesAsync();
            await _storageService.TouchAsync();
        }

        public async Task DeleteOneEntryAsync(Guid id)
        {
            var entity = await _appDbContext.MapEntries
                .Include(me => me.Links)
                .FirstOrDefaultAsync(entity => entity.Id == id);
            _appDbContext.Remove(entity);
            await _storageService.TouchAsync();
            await _appDbContext.SaveChangesAsync();
        }

        public async Task DeleteAllEntriesAsync(Guid id)
        {
            var entities = _appDbContext.MapEntries
                .Include(me => me.Links)
                .Where(entity => entity.MapId == id);

            _appDbContext.RemoveRange(entities);

            await _storageService.TouchAsync();
            await _appDbContext.SaveChangesAsync();
        }
    }
}