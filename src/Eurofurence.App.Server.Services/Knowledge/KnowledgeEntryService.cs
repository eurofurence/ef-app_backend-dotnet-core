﻿using System;
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
        public override async Task<KnowledgeEntryRecord> FindOneAsync(Guid id)
        {
            return await _appDbContext.KnowledgeEntries
                .Include(m => m.Images)
                .Include(m => m.Links)
                .AsNoTracking()
                .FirstOrDefaultAsync(entity => entity.Id == id);
        }

        public override IQueryable<KnowledgeEntryRecord> FindAll()
        {
            return _appDbContext.KnowledgeEntries
                .Include(m => m.Images)
                .Include(m => m.Links)
                .AsNoTracking();
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
            var existingEntity = await _appDbContext.KnowledgeEntries
                .Include(knowledgeEntry => knowledgeEntry.Links)
                .Include(knowledgeEntry => knowledgeEntry.Images)
                .FirstOrDefaultAsync(ke => ke.Id == id);

            existingEntity.KnowledgeGroupId = request.KnowledgeGroupId;
            existingEntity.Title = request.Title;
            existingEntity.Text = request.Text;
            existingEntity.Order = request.Order;
            existingEntity.Links = request.Links;

            foreach (var existingLink in existingEntity.Links)
            {
                if (request.Links.All(link => link.Id != existingLink.Id))
                {
                    _appDbContext.LinkFragments.Remove(existingLink);
                }
            }

            foreach (var link in request.Links)
            {
                var linkExists = await _appDbContext.LinkFragments.AnyAsync(existingLink => existingLink.Id == link.Id);
                if (!linkExists)
                {
                    _appDbContext.LinkFragments.Add(link);
                }
            }

            existingEntity.Images = await _appDbContext.Images.Where(image => request.ImageIds.Contains(image.Id)).ToListAsync();

            existingEntity.Touch();
            await _storageService.TouchAsync();
            await _appDbContext.SaveChangesAsync();

            return existingEntity;
        }
    }
}