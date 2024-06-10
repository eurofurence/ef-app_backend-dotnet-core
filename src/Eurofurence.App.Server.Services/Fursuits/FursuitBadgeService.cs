using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Eurofurence.App.Common.Utility;
using Eurofurence.App.Domain.Model.Fursuits;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Fursuits;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Threading;
using Eurofurence.App.Domain.Model.Images;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions.Images;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.ImageSharp.Formats.Jpeg;

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
            byte[] imageBytes = Convert.FromBase64String(registration.ImageContent);
            var hash = Hashing.ComputeHashSha1(imageBytes);

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
                record.Touch();

                var imageRecord = await _appDbContext.Images.FirstOrDefaultAsync(entity => entity.Id == record.ImageId);

                if (imageRecord == null)
                {
                    imageRecord = new ImageRecord
                    {
                        InternalReference = record.Id.ToString(),
                        Width = 240,
                        Height = 320,
                        MimeType = "image/jpeg"
                    };
                    imageRecord.Touch();
                }

                if (imageRecord.ContentHashSha1 != hash)
                {
                    imageRecord.ContentHashSha1 = hash;

                    var image = Image.Load(imageBytes);

                    image.Mutate(ctx =>
                        ctx.Resize(new ResizeOptions()
                        {
                            Mode = ResizeMode.Max,
                            Size = new Size(240, 320),
                            Sampler = new BicubicResampler()
                        })
                    );

                    var ms = new MemoryStream();
                    await image.SaveAsJpegAsync(ms, new JpegEncoder() { Quality = 85 });
                    imageRecord.SizeInBytes = ms.Length;
                    imageRecord.Touch();
                    var createdImage = await _imageService.InsertOrUpdateImageAsync(record.Id.ToString(), ms.ToArray());
                    record.ImageId = createdImage.Id;
                    await ms.DisposeAsync();
                }

                _appDbContext.FursuitBadges.Update(record);

                await _appDbContext.SaveChangesAsync();

                return record.Id;
            }
            catch (Exception ex)
            {
                if (fursuitBadge == null)
                {
                    return Guid.Empty;
                }

                var fursuitBadgeToDelete = await _appDbContext.FursuitBadges.SingleOrDefaultAsync(entity => entity.Id == fursuitBadge.Id);
                var fursuitBadgeImageToDelete = await _appDbContext.Images.SingleOrDefaultAsync(entity => entity.Id == fursuitBadge.ImageId);

                if (fursuitBadgeToDelete != null)
                {
                    _appDbContext.Remove(fursuitBadgeToDelete);
                }

                if (fursuitBadgeImageToDelete != null)
                {
                    await _imageService.DeleteOneAsync(fursuitBadgeImageToDelete.Id);
                }

                await _appDbContext.SaveChangesAsync();

                return Guid.Empty;
            }
            finally
            {
                _sync.Release();
            }
        }

        public async Task<byte[]> GetFursuitBadgeImageAsync(Guid id)
        {
            var content = await _appDbContext.FursuitBadges
                .FirstOrDefaultAsync(entity => entity.Id == id);
            byte[] result = null;

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
