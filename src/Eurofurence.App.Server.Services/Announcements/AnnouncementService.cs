using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Domain.Model.Sync;
using Eurofurence.App.Domain.Model.Transformers;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Announcements;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Announcements
{
    public class AnnouncementService : EntityServiceBase<AnnouncementRecord, AnnouncementResponse>,
        IAnnouncementService
    {
        private readonly AppDbContext _appDbContext;

        public AnnouncementService(
            AppDbContext appDbContext,
            IStorageServiceFactory storageServiceFactory
        )
            : base(appDbContext, storageServiceFactory)
        {
            _appDbContext = appDbContext;
        }

        public override async Task<DeltaResponse<AnnouncementResponse>> GetDeltaResponseAsync(
            DateTime? minLastDateTimeChangedUtc = null,
            CancellationToken cancellationToken = default)
        {

            var storageInfo = await GetStorageInfoAsync(cancellationToken);
            var response = new DeltaResponse<AnnouncementResponse>
            {
                StorageDeltaStartChangeDateTimeUtc = storageInfo.DeltaStartDateTimeUtc,
                StorageLastChangeDateTimeUtc = storageInfo.LastChangeDateTimeUtc
            };

            if (!minLastDateTimeChangedUtc.HasValue || minLastDateTimeChangedUtc < storageInfo.DeltaStartDateTimeUtc)
            {
                response.RemoveAllBeforeInsert = true;
                response.DeletedEntities = Array.Empty<Guid>();
                response.ChangedEntities = await
                    _appDbContext.Announcements
                        .Where(entity =>
                            entity.IsDeleted == 0)
                        .Select(x => x.Transform()).ToArrayAsync(cancellationToken);
            }
            else
            {
                response.RemoveAllBeforeInsert = false;

                var entities = _appDbContext.Announcements
                    .IgnoreQueryFilters()
                    .Where(entity => entity.LastChangeDateTimeUtc > minLastDateTimeChangedUtc);

                response.ChangedEntities = await entities
                    .Where(a => a.IsDeleted == 0)
                    .Select(x => x.Transform())
                    .ToArrayAsync(cancellationToken);
                response.DeletedEntities = await entities
                    .Where(a => a.IsDeleted == 1)
                    .Select(a => a.Id)
                    .ToArrayAsync(cancellationToken);
            }

            return response;
        }
    }
}