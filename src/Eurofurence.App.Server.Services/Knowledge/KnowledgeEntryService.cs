using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Eurofurence.App.Domain.Model.Knowledge;
using Eurofurence.App.Domain.Model.Sync;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Knowledge;
using Eurofurence.App.Server.Services.Abstractions.Sanitization;
using Microsoft.EntityFrameworkCore;

namespace Eurofurence.App.Server.Services.Knowledge
{
    public class KnowledgeEntryService : EntityServiceBase<KnowledgeEntryRecord>, IKnowledgeEntryService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IStorageService _storageService;
        private readonly IHtmlSanitizer _htmlSanitizer;

        public KnowledgeEntryService(
            AppDbContext appDbContext,
            IStorageServiceFactory storageServiceFactory,
            IHtmlSanitizer htmlSanitizer
        )
            : base(appDbContext, storageServiceFactory)
        {
            _appDbContext = appDbContext;
            _storageService = storageServiceFactory.CreateStorageService<KnowledgeEntryRecord>();
            _htmlSanitizer = htmlSanitizer;
        }
        public override async Task<KnowledgeEntryRecord> FindOneAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _appDbContext.KnowledgeEntries
                .Include(m => m.Images)
                .Include(m => m.Links)
                .AsNoTracking()
                .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);
        }

        public override IQueryable<KnowledgeEntryRecord> FindAll()
        {
            return _appDbContext.KnowledgeEntries
                .Include(m => m.Images)
                .Include(m => m.Links)
                .AsNoTracking();
        }

        public override Task ReplaceOneAsync(KnowledgeEntryRecord entity, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException();
        }

        public override Task InsertOneAsync(KnowledgeEntryRecord entity, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException();
        }

        public async Task<KnowledgeEntryRecord> InsertKnowledgeEntryAsync(
            KnowledgeEntryRequest request,
            CancellationToken cancellationToken = default)
        {
            var images = _appDbContext.Images.Where(image => request.ImageIds.Contains(image.Id));

            var entity = new KnowledgeEntryRecord
            {
                Id = request.Id,
                KnowledgeGroupId = request.KnowledgeGroupId,
                Title = request.Title,
                Text = HttpUtility.HtmlDecode(_htmlSanitizer.Sanitize(request.Text)),
                Order = request.Order,
                Links = request.Links,
                IsDeleted = 0
            };

            entity.Images.AddRange(images);

            entity.Touch();
            var result = await _appDbContext.AddAsync(entity, cancellationToken);
            await _storageService.TouchAsync(cancellationToken);
            await _appDbContext.SaveChangesAsync(cancellationToken);
            return result.Entity;
        }

        public async Task<KnowledgeEntryRecord> ReplaceKnowledgeEntryAsync(
            Guid id,
            KnowledgeEntryRequest request,
            CancellationToken cancellationToken = default)
        {
            var existingEntity = await _appDbContext.KnowledgeEntries
                .Include(knowledgeEntry => knowledgeEntry.Links)
                .Include(knowledgeEntry => knowledgeEntry.Images)
                .FirstOrDefaultAsync(ke => ke.Id == id, cancellationToken);

            existingEntity.KnowledgeGroupId = request.KnowledgeGroupId;
            existingEntity.Title = request.Title;
            existingEntity.Text = HttpUtility.HtmlDecode(_htmlSanitizer.Sanitize(request.Text));
            existingEntity.Order = request.Order;

            foreach (var existingLink in existingEntity.Links)
            {
                var entityLinkInNewEntity = request.Links.FirstOrDefault(link => Equals(link, existingLink));

                if (entityLinkInNewEntity != null)
                {
                    entityLinkInNewEntity.Id = existingLink.Id;
                }
                else
                {
                    _appDbContext.LinkFragments.Remove(existingLink);
                }
            }

            foreach (var link in request.Links)
            {
                if (!existingEntity.Links.Contains(link))
                {
                    _appDbContext.LinkFragments.Add(link);
                    existingEntity.Links.Add(link);
                }
            }

            existingEntity.Images = await _appDbContext.Images
                .Where(image => request.ImageIds.Contains(image.Id))
                .ToListAsync(cancellationToken);

            existingEntity.Touch();
            await _storageService.TouchAsync(cancellationToken);
            await _appDbContext.SaveChangesAsync(cancellationToken);

            return existingEntity;
        }

        public override async Task<DeltaResponse<KnowledgeEntryRecord>> GetDeltaResponseAsync(
            DateTime? minLastDateTimeChangedUtc = null,
            CancellationToken cancellationToken = default)
        {
            var storageInfo = await GetStorageInfoAsync(cancellationToken);
            var response = new DeltaResponse<KnowledgeEntryRecord>
            {
                StorageDeltaStartChangeDateTimeUtc = storageInfo.DeltaStartDateTimeUtc,
                StorageLastChangeDateTimeUtc = storageInfo.LastChangeDateTimeUtc
            };

            if (!minLastDateTimeChangedUtc.HasValue || minLastDateTimeChangedUtc < storageInfo.DeltaStartDateTimeUtc)
            {
                response.RemoveAllBeforeInsert = true;
                response.DeletedEntities = Array.Empty<Guid>();
                response.ChangedEntities = await
                    _appDbContext.KnowledgeEntries
                        .Include(ke => ke.Links)
                        .Include(ke => ke.Images)
                        .Where(entity => entity.IsDeleted == 0)
                        .ToArrayAsync(cancellationToken);
            }
            else
            {
                response.RemoveAllBeforeInsert = false;

                var entities = _appDbContext.KnowledgeEntries
                    .Include(ke => ke.Links)
                    .Include(ke => ke.Images)
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
    }
}