using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Eurofurence.App.Common.Utility;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Images;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Images;
using SixLabors.ImageSharp;
using System.IO;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using Eurofurence.App.Domain.Model.Fragments;

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
            var hash = Hashing.ComputeHashSha1(imageBytes);

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

            var image = Image.Load(imageBytes, out IImageFormat imageFormat);

            var record = new ImageRecord
            {
                Id = existingRecord?.Id ?? Guid.NewGuid(),
                IsDeleted = 0,
                InternalReference = internalReference,
                MimeType = imageFormat.DefaultMimeType,
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

        public byte[] GeneratePlaceholderImage()
        {
            var image = new Image<Rgba32>(1, 1);
            var output = new MemoryStream();
            image.SaveAsPng(output);

            return output.ToArray();           
        }

        public async Task InsertImageAsync(ImageRecord image, byte[] imageBytes)
        {
            await _imageRepository.InsertOneAsync(image);
            await InsertOrUpdateImageAsync(image.InternalReference, imageBytes);
        }

        public ImageFragment GenerateFragmentFromBytes(byte[] imageBytes)
        {
            try
            {
                var image = Image.Load(imageBytes, out IImageFormat imageFormat);
                return new ImageFragment()
                {
                    SizeInBytes = imageBytes.LongLength,
                    Height = image.Height,
                    Width = image.Width,
                    MimeType = imageFormat.DefaultMimeType,
                    ImageBytes = imageBytes
                };
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}