using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Images;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Images;
using ImageSharp;

namespace Eurofurence.App.Server.Services.Images
{
    public class ImageService : EntityServiceBase<ImageRecord>, IImageService
    {
        private readonly IEntityRepository<ImageContentRecord> _imageContentRepository;
        private readonly IEntityRepository<ImageRecord> _imageRepository;
        private readonly IStorageServiceFactory _storageServiceFactory;

        public ImageService(
            IEntityRepository<ImageRecord> imageRepository,
            IEntityRepository<ImageContentRecord> imageContentRepository,
            IStorageServiceFactory storageServiceFactory)
            : base(imageRepository, storageServiceFactory)
        {
            _storageServiceFactory = storageServiceFactory;
            _imageContentRepository = imageContentRepository;
            _imageRepository = imageRepository;
        }

        public override Task ReplaceOneAsync(ImageRecord entity)
        {
            throw new InvalidOperationException();
        }

        public override Task InsertOneAsync(ImageRecord entity)
        {
            throw new InvalidOperationException();
        }

        public override async Task DeleteOneAsync(Guid id)
        {
            await _imageContentRepository.DeleteOneAsync(id);
            await base.DeleteOneAsync(id);
        }

        public override async Task DeleteAllAsync()
        {
            await _imageContentRepository.DeleteAllAsync();
            await base.DeleteAllAsync();
        }

        public async Task<Guid> InsertOrUpdateImageAsync(string internalReference, byte[] imageBytes)
        {
            var hash = CalculateSha1Hash(imageBytes);

            var existingRecord = (await _imageRepository.FindAllAsync(a => a.InternalReference == internalReference))
                .ToList()
                .SingleOrDefault();

            if (existingRecord != null && existingRecord.ContentHashSha1 == hash)
            {
                // Ensure we still have the image!
                var existingContentRecord = await _imageContentRepository.FindOneAsync(existingRecord.Id);
                if (existingContentRecord == null)
                {
                    await _imageContentRepository.InsertOneAsync(new ImageContentRecord
                    {
                        Id = existingRecord.Id,
                        IsDeleted = 0,
                        Content = imageBytes
                    });
                }

                return existingRecord.Id;
            }
                

            var image = Image.Load(imageBytes);

            var record = new ImageRecord
            {
                Id = existingRecord?.Id ?? Guid.NewGuid(),
                IsDeleted = 0,
                InternalReference = internalReference,
                MimeType = image.CurrentImageFormat.MimeType,
                Width = image.Width,
                Height = image.Height,
                SizeInBytes = imageBytes.Length,
                ContentHashSha1 = hash
            };

            var contentRecord = new ImageContentRecord
            {
                Id = record.Id,
                IsDeleted = 0,
                Content = imageBytes
            };

            record.Touch();
            contentRecord.Touch();

            if (existingRecord != null)
            {
                await base.ReplaceOneAsync(record);
                await _imageContentRepository.ReplaceOneAsync(contentRecord);
            }
            else
            {
                await base.InsertOneAsync(record);
                await _imageContentRepository.InsertOneAsync(contentRecord);
            }

            return record.Id;
        }

        public async Task<byte[]> GetImageContentByIdAsync(Guid id)
        {
            var record = await _imageContentRepository.FindOneAsync(id);
            return record.Content;
        }

        private string CalculateSha1Hash(byte[] bytes)
        {
            using (var sha1 = SHA1.Create())
            {
                return Convert.ToBase64String(sha1.ComputeHash(bytes));
            }
        }
    }
}