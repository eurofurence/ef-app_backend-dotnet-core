using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Eurofurence.App.Common.Utility;
using Eurofurence.App.Domain.Model.Fursuits;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Fursuits;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Threading;
using Eurofurence.App.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace Eurofurence.App.Server.Services.Fursuits
{
    public class FursuitBadgeService : IFursuitBadgeService
    {
        private readonly ConventionSettings _conventionSettings;
        private readonly AppDbContext _appDbContext;
        private readonly SemaphoreSlim _sync = new(1, 1);

        public FursuitBadgeService(
            AppDbContext appDbContext,
            ConventionSettings conventionSettings
            )
        {
            _appDbContext = appDbContext;
            _conventionSettings = conventionSettings;
        }

        public async Task<Guid> UpsertFursuitBadgeAsync(FursuitBadgeRegistration registration)
        {
            byte[] imageBytes = Convert.FromBase64String(registration.ImageContent);
            var hash = Hashing.ComputeHashSha1(imageBytes);

            await _sync.WaitAsync();

            Guid id = Guid.Empty;

            try
            {
                var record = await _appDbContext.FursuitBadges.FirstOrDefaultAsync(a => a.ExternalReference == registration.BadgeNo.ToString());

                if (record == null)
                {
                    record = new FursuitBadgeRecord();
                    record.NewId();

                    _appDbContext.FursuitBadges.Add(record);
                }

                id = record.Id;

                record.ExternalReference = registration.BadgeNo.ToString();
                record.OwnerUid = $"RegSys:{_conventionSettings.ConventionIdentifier}:{registration.RegNo}";
                record.Gender = registration.Gender;
                record.Name = registration.Name;
                record.Species = registration.Species;
                record.IsPublic = (registration.DontPublish == 0);
                record.WornBy = registration.WornBy;
                record.CollectionCode = registration.CollectionCode;
                record.Touch();

                var imageRecord = await _appDbContext.FursuitBadgeImages.FirstOrDefaultAsync(entity => entity.Id == record.Id);

                if (imageRecord == null)
                {
                    imageRecord = new FursuitBadgeImageRecord
                    {
                        Id = record.Id,
                        Width = 240,
                        Height = 320,
                        MimeType = "image/jpeg"
                    };
                    imageRecord.Touch();

                    _appDbContext.FursuitBadgeImages.Add(imageRecord);
                }

                if (imageRecord.SourceContentHashSha1 != hash)
                {
                    imageRecord.SourceContentHashSha1 = hash;

                    var image = Image.Load(imageBytes);

                    image.Mutate(ctx =>
                        ctx.Resize(new ResizeOptions()
                        {
                            Mode = ResizeMode.Max,
                            Size = new SixLabors.ImageSharp.Size(240, 320),
                            Sampler = new BicubicResampler()
                        })
                    );

                    var ms = new MemoryStream();
                    image.SaveAsJpeg(ms, new JpegEncoder() { Quality = 85 });
                    imageRecord.SizeInBytes = ms.Length;
                    imageRecord.ImageBytes = ms.ToArray();
                    ms.Dispose();

                    imageRecord.Touch();
                    _appDbContext.FursuitBadgeImages.Update(imageRecord);
                }

                _appDbContext.FursuitBadges.Update(record);

                await _appDbContext.SaveChangesAsync();

                return record.Id;
            }
            catch (Exception)
            {
                if (id == Guid.Empty)
                {
                    return id;
                }

                var fursuitBadgeToDelete = await _appDbContext.FursuitBadges.SingleOrDefaultAsync(entity => entity.Id == id);
                var fursuitBadgeImageToDelete = await _appDbContext.FursuitBadgeImages.SingleOrDefaultAsync(entity => entity.Id == id);

                if (fursuitBadgeToDelete != null)
                {
                    _appDbContext.Remove(fursuitBadgeToDelete);
                }

                if (fursuitBadgeImageToDelete != null)
                {
                    _appDbContext.Remove(fursuitBadgeImageToDelete);
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
            var content = await _appDbContext.FursuitBadgeImages.FirstOrDefaultAsync(entity => entity.Id == id);
            return content?.ImageBytes;
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
                        && record.Name.ToLowerInvariant().Contains(filter.Name.ToLowerInvariant()));
        }
    }
}
