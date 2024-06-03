using System;
using System.Collections.Generic;
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
using Eurofurence.App.Server.Services.Abstractions.MinIO;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.ImageSharp.Formats.Jpeg;
using Microsoft.EntityFrameworkCore;
using Minio;
using Minio.DataModel.Args;

namespace Eurofurence.App.Server.Services.Images
{
    public class ImageService : EntityServiceBase<ImageRecord>, IImageService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMinioClient _minIoClient;
        private readonly MinIoConfiguration _minIoConfiguration;

        public ImageService(
            AppDbContext appDbContext,
            IStorageServiceFactory storageServiceFactory,
            MinIoConfiguration minIoConfiguration)
            : base(appDbContext, storageServiceFactory, false)
        {
            _appDbContext = appDbContext;
            _minIoConfiguration = minIoConfiguration;
            _minIoClient = new MinioClient().WithEndpoint(minIoConfiguration.Endpoint)
                .WithCredentials(minIoConfiguration.AccessKey, minIoConfiguration.SecretKey)
                .WithSSL(minIoConfiguration.Secure)
                .Build();
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
            await DeleteFileFromMinIoAsync(_minIoConfiguration.Bucket, id.ToString());
            await base.DeleteOneAsync(id);
        }

        public override async Task DeleteAllAsync()
        {
            var imageIds = await _appDbContext.Images.Select(image => image.Id.ToString()).ToListAsync();
            await DeleteFilesFromMinIoAsync(_minIoConfiguration.Bucket, imageIds);
            await base.DeleteAllAsync();
        }

        public async Task<ImageRecord> InsertOrUpdateImageAsync(string internalReference, byte[] imageBytes)
        {
            var hash = Hashing.ComputeHashSha1(imageBytes);

            var image = Image.Load(imageBytes);
            IImageFormat imageFormat = image.Metadata.DecodedImageFormat;

            var existingRecord = await
                _appDbContext.Images
                    .AsNoTracking()
                    .FirstOrDefaultAsync(entity => entity.InternalReference == internalReference);

            if (existingRecord != null && existingRecord.ContentHashSha1 == hash)
            {
                // Ensure we still have the image!
                var objectExists = await _minIoClient.StatObjectAsync(new StatObjectArgs()
                    .WithBucket(_minIoConfiguration.Bucket)
                    .WithObject(existingRecord.Id.ToString()));

                if (objectExists == null)
                {
                    await UploadFileToMinIoAsync(_minIoConfiguration.Bucket, existingRecord.Id.ToString(),
                        imageFormat?.DefaultMimeType, imageBytes);
                }

                return existingRecord;
            }

            var record = new ImageRecord
            {
                Id = existingRecord?.Id ?? Guid.NewGuid(),
                IsDeleted = 0,
                InternalReference = internalReference,
                MimeType = imageFormat?.DefaultMimeType,
                Width = image.Width,
                Height = image.Height,
                SizeInBytes = imageBytes.Length,
                ContentHashSha1 = hash
            };

            record.Touch();

            if (existingRecord != null)
            {
                await base.ReplaceOneAsync(record);
            }
            else
            {
                await base.InsertOneAsync(record);
            }

            await UploadFileToMinIoAsync(_minIoConfiguration.Bucket, record.Id.ToString(),
                imageFormat?.DefaultMimeType, imageBytes);

            await _appDbContext.SaveChangesAsync();

            return record;
        }

        public async Task<byte[]> GetImageContentByImageIdAsync(Guid id)
        {
            // Checks if the object exists
            await _minIoClient.StatObjectAsync(new StatObjectArgs()
                .WithBucket(_minIoConfiguration.Bucket)
                .WithObject(id.ToString()));

            using var ms = new MemoryStream();

            await _minIoClient.GetObjectAsync(new GetObjectArgs()
                .WithBucket(_minIoConfiguration.Bucket)
                .WithObject(id.ToString())
                .WithCallbackStream(stream =>
                {
                    stream.CopyTo(ms);
                }));
            return ms.ToArray();
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
                    MimeType = imageFormat?.DefaultMimeType,
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
            rawImage.SaveAsJpeg(ms, new JpegEncoder() { Quality = 85 });
            var newFragment = GenerateFragmentFromBytes(ms.ToArray());
            ms.Dispose();

            return newFragment;
        }

        private async Task UploadFileToMinIoAsync(string bucketName, string objectName, string contentType, byte[] content)
        {
            // Make a bucket on the server, if not already present
            var found = await _minIoClient
                .BucketExistsAsync(new BucketExistsArgs()
                    .WithBucket(bucketName))
                .ConfigureAwait(false);
            if (!found)
            {
                var mbArgs = new MakeBucketArgs()
                    .WithBucket(bucketName);
                await _minIoClient.MakeBucketAsync(mbArgs).ConfigureAwait(false);
            }

            using var ms = new MemoryStream(content);
            // Upload a file to bucket
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithObjectSize(ms.Length)
                .WithStreamData(ms)
                .WithContentType(contentType);
            await _minIoClient.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
        }

        private async Task DeleteFileFromMinIoAsync(string bucketName, string objectName)
        {
            // If the bucket does not exist, return
            var found = await _minIoClient
                .BucketExistsAsync(new BucketExistsArgs()
                    .WithBucket(bucketName))
                .ConfigureAwait(false);
            if (!found)
            {
                return;
            }

            // Remove the file from bucket
            var removeObjectArgs = new RemoveObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName);
            await _minIoClient.RemoveObjectAsync(removeObjectArgs).ConfigureAwait(false);
        }

        private async Task DeleteFilesFromMinIoAsync(string bucketName, List<string> objectNames)
        {
            // If the bucket does not exist, return
            var found = await _minIoClient
                .BucketExistsAsync(new BucketExistsArgs()
                    .WithBucket(bucketName))
                .ConfigureAwait(false);
            if (!found)
            {
                return;
            }

            // Remove the file from bucket
            var removeObjectsArgs = new RemoveObjectsArgs()
                .WithBucket(bucketName)
                .WithObjects(objectNames);
            await _minIoClient.RemoveObjectsAsync(removeObjectsArgs).ConfigureAwait(false);
        }
    }
}