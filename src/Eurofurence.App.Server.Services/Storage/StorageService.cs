using System;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Sync;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Eurofurence.App.Server.Services.Storage
{
    public class StorageService<T> : IStorageService
    {
        private readonly AppDbContext _appDbContext;
        private readonly string _entityType;

        public StorageService(AppDbContext appDbContext, string entityType)
        {
            _appDbContext = appDbContext;
            _entityType = entityType;
        }


        public async Task TouchAsync()
        {
            var record = await GetEntityStorageRecordAsync();
            record.LastChangeDateTimeUtc = DateTime.UtcNow;
            _appDbContext.EntityStorageInfos.Update(record);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task ResetDeltaStartAsync()
        {
            var record = await GetEntityStorageRecordAsync();
            record.DeltaStartDateTimeUtc = DateTime.UtcNow;
            _appDbContext.EntityStorageInfos.Update(record);
            await _appDbContext.SaveChangesAsync();
        }

        public Task<EntityStorageInfoRecord> GetStorageInfoAsync()
        {
            return GetEntityStorageRecordAsync();
        }

        private async Task<EntityStorageInfoRecord> GetEntityStorageRecordAsync()
        {
            var record = await _appDbContext.EntityStorageInfos.FirstOrDefaultAsync(entity => entity.EntityType == _entityType);

            if (record == null)
            {
                record = new EntityStorageInfoRecord
                {
                    EntityType = _entityType,
                    DeltaStartDateTimeUtc = DateTime.UtcNow
                };

                record.NewId();
                record.Touch();

                _appDbContext.EntityStorageInfos.Add(record);
                await _appDbContext.SaveChangesAsync();
            }

            return record;
        }
    }
}