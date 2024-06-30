using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Fursuits;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Fursuits;
using System.Threading;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Microsoft.EntityFrameworkCore;

namespace Eurofurence.App.Server.Services.Fursuits
{
    public class FursuitBadgeService : IFursuitBadgeService
    {
        private readonly ConventionSettings _conventionSettings;
        private readonly AppDbContext _appDbContext;
        private readonly IImageService _imageService;
        private readonly SemaphoreSlim _sync = new(1, 1);

        public FursuitBadgeService(
            AppDbContext appDbContext,
            ConventionSettings conventionSettings,
            IImageService imageService
            )
        {
            _appDbContext = appDbContext;
            _conventionSettings = conventionSettings;
            _imageService = imageService;
        }

        public async Task<Guid> UpsertFursuitBadgeAsync(FursuitBadgeRegistration registration)
        {
            await _sync.WaitAsync();

            FursuitBadgeRecord? fursuitBadge = null;

            try
            {
                var record = await _appDbContext.FursuitBadges.FirstOrDefaultAsync(a => a.ExternalReference == registration.BadgeNo.ToString());

                if (record == null)
                {
                    record = new FursuitBadgeRecord();
                    record.NewId();

                    _appDbContext.FursuitBadges.Add(record);
                }

                fursuitBadge = record;

                record.ExternalReference = registration.BadgeNo.ToString();
                record.OwnerUid = $"RegSys:{_conventionSettings.ConventionIdentifier}:{registration.RegNo}";
                record.Gender = registration.Gender;
                record.Name = registration.Name;
                record.Species = registration.Species;
                record.IsPublic = (registration.DontPublish == 0);
                record.WornBy = registration.WornBy;
                record.CollectionCode = registration.CollectionCode;
                record.ImageId = registration.ImageId;
                record.Touch();

                var imageRecord = await _appDbContext.Images.AsNoTracking().FirstOrDefaultAsync(entity => entity.Id == record.ImageId);

                await _imageService.EnforceMaximumDimensionsAsync(imageRecord, 240, 320);

                _appDbContext.FursuitBadges.Update(record);

                await _appDbContext.SaveChangesAsync();

                return record.Id;
            }
            catch (Exception ex)
            {
                return Guid.Empty;
            }
            finally
            {
                _sync.Release();
            }
        }

        public async Task<Stream> GetFursuitBadgeImageAsync(Guid id)
        {
            var content = await _appDbContext.FursuitBadges
                .FirstOrDefaultAsync(entity => entity.Id == id);
            Stream result = null;

            if (content.ImageId != null)
            {
                result = await _imageService.GetImageContentByImageIdAsync((Guid)content.ImageId);
            }
            
            return result;
        }

        public IQueryable<FursuitBadgeRecord> GetFursuitBadges(FursuitBadgeFilter filter = null)
        {
            return filter == null ?
                _appDbContext.FursuitBadges
                    .AsNoTracking() :

                string.IsNullOrWhiteSpace(filter.Name) ?
                    _appDbContext.FursuitBadges
                        .AsNoTracking()
                        .Where(record =>
                        (string.IsNullOrWhiteSpace(filter.ExternalReference) || record.ExternalReference.Equals(filter.ExternalReference))
                        && (string.IsNullOrWhiteSpace(filter.OwnerUid) || record.OwnerUid.Equals(filter.OwnerUid)))
                    :
                    _appDbContext.FursuitBadges
                        .AsNoTracking()
                        .Where(record =>
                        (string.IsNullOrWhiteSpace(filter.ExternalReference) || record.ExternalReference.Equals(filter.ExternalReference))
                        && (string.IsNullOrWhiteSpace(filter.OwnerUid) || record.OwnerUid.Equals(filter.OwnerUid))
                        && EF.Functions.Like(record.Name, $"%{filter.Name}%"));
        }
    }
}
