using System;
using System.Threading.Tasks;
using Eurofurence.App.Common.Utility;
using Eurofurence.App.Domain.Model.Images;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Images;
using SixLabors.ImageSharp;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using Eurofurence.App.Domain.Model.Fragments;
using Eurofurence.App.Infrastructure.EntityFramework;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.ImageSharp.Formats.Jpeg;
using Eurofurence.App.Server.Services.Storage;
using Microsoft.EntityFrameworkCore;

namespace Eurofurence.App.Server.Services.Images
{
    public class ImageService : EntityServiceBase<ImageRecord>, IImageService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IStorageServiceFactory _storageServiceFactory;

        public ImageService(
            AppDbContext appDbContext,
            IStorageServiceFactory storageServiceFactory)
            : base(appDbContext, storageServiceFactory)
        {
            _appDbContext = appDbContext;
            _storageServiceFactory = storageServiceFactory;
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
            var entity = await _appDbContext.ImageContents.FirstOrDefaultAsync(entity => entity.Id == id);
            _appDbContext.Remove(entity);
            await _appDbContext.SaveChangesAsync();
            await base.DeleteOneAsync(id);
        }

        public override async Task DeleteAllAsync()
        {
            _appDbContext.RemoveRange(_appDbContext.ImageContents);
            await _appDbContext.SaveChangesAsync();
            await base.DeleteAllAsync();
        }

        public async Task<Guid> InsertOrUpdateImageAsync(string internalReference, byte[] imageBytes)
        {
            var hash = Hashing.ComputeHashSha1(imageBytes);

            var existingRecord = await
                _appDbContext.Images.FirstOrDefaultAsync(entity => entity.InternalReference == internalReference);

            if (existingRecord != null && existingRecord.ContentHashSha1 == hash)
            {
                // Ensure we still have the image!
                var existingContentRecord = await _appDbContext.ImageContents.FirstOrDefaultAsync(entity => entity.Id == existingRecord.Id);
                if (existingContentRecord == null)
                {
                    _appDbContext.ImageContents.Add(new ImageContentRecord
                    {
                        Id = existingRecord.Id,
                        IsDeleted = 0,
                        Content = imageBytes
                    });
                    await _appDbContext.SaveChangesAsync();
                }

                return existingRecord.Id;
            }

            var image = Image.Load(imageBytes);
            IImageFormat imageFormat = image.Metadata.DecodedImageFormat;

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
                _appDbContext.ImageContents.Update(contentRecord);
            }
            else
            {
                await base.InsertOneAsync(record);
                _appDbContext.ImageContents.Add(contentRecord);
            }

            await _appDbContext.SaveChangesAsync();

            return record.Id;
        }

        public async Task<byte[]> GetImageContentByIdAsync(Guid id)
        {
            var record = await _appDbContext.ImageContents.FirstOrDefaultAsync(entity => entity.Id == id);
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
            _appDbContext.Images.Add(image);
            await _appDbContext.SaveChangesAsync();
            await InsertOrUpdateImageAsync(image.InternalReference, imageBytes);
        }

        public ImageFragment GenerateFragmentFromBytes(byte[] imageBytes)
        {
            if (imageBytes == null || imageBytes.Length == 0) return null;

            try
            {
                var image = Image.Load(imageBytes);
                IImageFormat imageFormat = image.Metadata.DecodedImageFormat;
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

        public ImageFragment EnforceMaximumDimensions(ImageFragment image, int width, int height)
        {
            if (image == null) return null;

            double scaling = Math.Min((double)width / image.Width, (double)height / image.Height);
            if (scaling >= 1) return image;

            var rawImage = Image.Load(image.ImageBytes);

            rawImage.Mutate(ctx =>
                ctx.Resize(new ResizeOptions()
                {
                    Mode = ResizeMode.Max,
                    Size = new SixLabors.ImageSharp.Size(width, height),
                    Sampler = new BicubicResampler()
                })
            );

            var ms = new MemoryStream();
            rawImage.SaveAsJpeg(ms, new JpegEncoder() {  Quality = 85 });
            var newFragment = GenerateFragmentFromBytes(ms.ToArray());
            ms.Dispose();

            return newFragment;
        }
    }
}