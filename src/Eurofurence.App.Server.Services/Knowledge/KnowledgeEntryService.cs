using System;
using System.Linq;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Knowledge;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Knowledge;
using Microsoft.EntityFrameworkCore;

namespace Eurofurence.App.Server.Services.Knowledge
{
    public class KnowledgeEntryService : EntityServiceBase<KnowledgeEntryRecord>,
        IKnowledgeEntryService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IStorageService _storageService;

        public KnowledgeEntryService(
            AppDbContext appDbContext,
            IStorageServiceFactory storageServiceFactory
        )
            : base(appDbContext, storageServiceFactory)
        {
            _appDbContext = appDbContext;
            _storageService = storageServiceFactory.CreateStorageService<KnowledgeEntryRecord>();
        }
        public override Task ReplaceOneAsync(KnowledgeEntryRecord entity)
        {
            throw new InvalidOperationException();
        }

        public override Task InsertOneAsync(KnowledgeEntryRecord entity)
        {
            throw new InvalidOperationException();
        }

        public async Task<KnowledgeEntryRecord> InsertKnowledgeEntryAsync(KnowledgeEntryRequest request)
        {
            var images = _appDbContext.Images.Where(image => request.ImageIds.Contains(image.Id));
            
            var entity = new KnowledgeEntryRecord
            {
                KnowledgeGroupId = request.KnowledgeGroupId,
                Title = request.Title,
                Text = request.Text,
                Order = request.Order,
                Links = request.Links,
                IsDeleted = 0
            };

            entity.Images.AddRange(images);

            entity.Touch();
            var result = await _appDbContext.AddAsync(entity);
            await _storageService.TouchAsync();
            await _appDbContext.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<KnowledgeEntryRecord> ReplaceKnowledgeEntryAsync(Guid id, KnowledgeEntryRequest request)
        {
            var images = _appDbContext.Images.Where(image => request.ImageIds.Contains(image.Id));

            var existingEntity = await _appDbContext.KnowledgeEntries
                .Include(knowledgeEntryRecord => knowledgeEntryRecord.Links)
                .AsNoTracking()
                .FirstOrDefaultAsync(ke => ke.Id == id);

            var entity = new KnowledgeEntryRecord
            {
                Id = id,
                KnowledgeGroupId = request.KnowledgeGroupId,
                Title = request.Title,
                Text = request.Text,
                Order = request.Order,
                Links = request.Links,
                IsDeleted = 0
            };

            foreach (var existingLink in existingEntity.Links)
            {
                if (!entity.Links.Contains(existingLink))
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

            entity.Images = [.. images];

            entity.Touch();
            var result = _appDbContext.KnowledgeEntries.Update(entity);
            await _storageService.TouchAsync();
            await _appDbContext.SaveChangesAsync();

            return result.Entity;
        }
    }
}