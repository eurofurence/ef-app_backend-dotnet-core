using System;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Sync;
using Eurofurence.App.Server.Services.Abstractions;

namespace Eurofurence.App.Server.Services.Storage
{
    public class StorageService<T> : IStorageService
    {
        private readonly IEntityStorageInfoRepository _entityStorageInfoRepository;
        private readonly string _entityType;

        public StorageService(IEntityStorageInfoRepository entityStorageInfoRepository, string entityType)
        {
            _entityStorageInfoRepository = entityStorageInfoRepository;
            _entityType = entityType;
        }


        public async Task TouchAsync()
        {
            var record = await GetEntityStorageRecordAsync();
            record.LastChangeDateTimeUtc = DateTime.UtcNow;
            await _entityStorageInfoRepository.ReplaceOneAsync(record);
        }

        public async Task ResetDeltaStartAsync()
        {
            var record = await GetEntityStorageRecordAsync();
            record.DeltaStartDateTimeUtc = DateTime.UtcNow;
            await _entityStorageInfoRepository.ReplaceOneAsync(record);
        }

        public Task<EntityStorageInfoRecord> GetStorageInfoAsync()
        {
            return GetEntityStorageRecordAsync();
        }

        private async Task<EntityStorageInfoRecord> GetEntityStorageRecordAsync()
        {
            var record = await _entityStorageInfoRepository.FindOneAsync(_entityType);

            if (record == null)
            {
                record = new EntityStorageInfoRecord {EntityType = _entityType};
                record.NewId();
                record.Touch();

                await _entityStorageInfoRepository.InsertOneAsync(record);
            }

            return record;
        }
    }
}