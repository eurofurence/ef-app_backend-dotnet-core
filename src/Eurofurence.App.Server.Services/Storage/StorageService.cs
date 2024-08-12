using System;
using System.Threading;
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


        public async Task TouchAsync(CancellationToken cancellationToken = default)
        {
            var record = await GetEntityStorageRecordAsync(cancellationToken);
            record.LastChangeDateTimeUtc = DateTime.UtcNow;
            _appDbContext.EntityStorageInfos.Update(record);
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task ResetDeltaStartAsync(CancellationToken cancellationToken = default)
        {
            var record = await GetEntityStorageRecordAsync(cancellationToken);
            record.DeltaStartDateTimeUtc = DateTime.UtcNow;
            _appDbContext.EntityStorageInfos.Update(record);
            await _appDbContext.SaveChangesAsync(cancellationToken);
        }

        public Task<EntityStorageInfoRecord> GetStorageInfoAsync(CancellationToken cancellationToken = default)
        {
            return GetEntityStorageRecordAsync(cancellationToken);
        }

        private async Task<EntityStorageInfoRecord> GetEntityStorageRecordAsync(CancellationToken cancellationToken = default)
        {
            var record = await _appDbContext.EntityStorageInfos
                .FirstOrDefaultAsync(entity => entity.EntityType == _entityType, cancellationToken);

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
                await _appDbContext.SaveChangesAsync(cancellationToken);
            }

            return record;
        }
    }
}