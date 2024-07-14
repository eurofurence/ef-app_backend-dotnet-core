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
        private readonly IStorageService _storageService;
        private readonly IMinioClient _minIoClient;
        private readonly MinIoConfiguration _minIoConfiguration;

        public ImageService(
            AppDbContext appDbContext,
            IStorageServiceFactory storageServiceFactory,
            MinIoConfiguration minIoConfiguration)
            : base(appDbContext, storageServiceFactory)
        {
            _appDbContext = appDbContext;
            _storageService = storageServiceFactory.CreateStorageService<ImageRecord>();
            _minIoConfiguration = minIoConfiguration;
            _minIoClient = new MinioClient().WithEndpoint(minIoConfiguration.Endpoint)
                .WithCredentials(minIoConfiguration.AccessKey, minIoConfiguration.SecretKey)
                .WithRegion(minIoConfiguration.Region)
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

            var entity = await _appDbContext.Images
                .Include(i => i.FursuitBadges)
                .Include(i => i.TableRegistrations)
                .FirstOrDefaultAsync(entity => entity.Id == id);

            _appDbContext.Remove(entity);

            await _storageService.TouchAsync();
            await _appDbContext.SaveChangesAsync();
        }

        public override async Task DeleteAllAsync()
        {
            var imageIds = await _appDbContext.Images.Select(image => image.Id.ToString()).ToListAsync();
            await DeleteFilesFromMinIoAsync(_minIoConfiguration.Bucket, imageIds);
            await base.DeleteAllAsync();
        }

        public async Task<ImageRecord> InsertImageAsync(string internalReference, Stream stream)
        {
            string hash;
            Image image;
            using (MemoryStream ms = new())
            {
                stream.Position = 0;
                await stream.CopyToAsync(ms);
                var byteArray = ms.ToArray();
                hash = Hashing.ComputeHashSha1(byteArray);
                image = Image.Load(byteArray);
            }

            var imageFormat = image.Metadata.DecodedImageFormat;

            var record = new ImageRecord
            {
                InternalReference = internalReference,
                IsDeleted = 0,
                MimeType = imageFormat?.DefaultMimeType,
                Width = image.Width,
                Height = image.Height,
                SizeInBytes = stream.Length,
                ContentHashSha1 = hash
            };

            record.Touch();

            await base.InsertOneAsync(record);

            await UploadFileToMinIoAsync(_minIoConfiguration.Bucket, record.Id.ToString(),
                imageFormat?.DefaultMimeType, stream);

            await _appDbContext.SaveChangesAsync();

            return record;
        }

        public async Task<ImageRecord> ReplaceImageAsync(Guid id, string internalReference, Stream stream)
        {
            Image image;
            using (MemoryStream ms = new())
            {
                stream.Position = 0;
                await stream.CopyToAsync(ms);
                var byteArray = ms.ToArray();
                image = Image.Load(byteArray);
            }

            IImageFormat imageFormat = image.Metadata.DecodedImageFormat;

            var existingRecord = await
                _appDbContext.Images
                    .AsNoTracking()
                    .FirstOrDefaultAsync(entity => entity.Id == id);

            await UploadFileToMinIoAsync(_minIoConfiguration.Bucket, existingRecord.Id.ToString(),
                imageFormat?.DefaultMimeType, stream);

            existingRecord.InternalReference = internalReference;
            existingRecord.MimeType = imageFormat?.DefaultMimeType;
            existingRecord.Width = image.Width;
            existingRecord.Height = image.Height;
            existingRecord.SizeInBytes = stream.Length;

            await base.ReplaceOneAsync(existingRecord);
            return existingRecord;
        }

        public async Task<Stream> GetImageContentByImageIdAsync(Guid id)
        {
            // Checks if the object exists
            await _minIoClient.StatObjectAsync(new StatObjectArgs()
                .WithBucket(_minIoConfiguration.Bucket)
                .WithObject(id.ToString()));

            var ms = new MemoryStream();
            await _minIoClient.GetObjectAsync(new GetObjectArgs()
                .WithBucket(_minIoConfiguration.Bucket)
                .WithObject(id.ToString())
                .WithCallbackStream(stream =>
                {
                    stream.CopyTo(ms);
                }));

            ms.Position = 0;
            return ms;
        }

        public Stream GeneratePlaceholderImage()
        {
            var image = new Image<Rgba32>(1, 1);
            var output = new MemoryStream();
            image.SaveAsPng(output);

            return output;
        }

        public async Task<ImageRecord> EnforceMaximumDimensionsAsync(ImageRecord image, int width, int height)
        {
            if (image == null) return null;

            double scaling = Math.Min((double)width / image.Width, (double)height / image.Height);
            if (scaling >= 1) return image;

            var stream = await GetImageContentByImageIdAsync(image.Id);

            Image rawImage;
            using (MemoryStream ms = new())
            {
                stream.Position = 0;
                await stream.CopyToAsync(ms);
                var byteArray = ms.ToArray();
                rawImage = Image.Load(byteArray);
            }

            await stream.DisposeAsync();
            rawImage.Mutate(ctx =>
                ctx.Resize(new ResizeOptions()
                {
                    Mode = ResizeMode.Max,
                    Size = new SixLabors.ImageSharp.Size(width, height),
                    Sampler = new BicubicResampler()
                })
            );

            using (MemoryStream resizedImageStream = new())
            {
                await rawImage.SaveAsJpegAsync(resizedImageStream, new JpegEncoder { Quality = 85 });
                var newImage = await ReplaceImageAsync(image.Id, image.InternalReference, resizedImageStream);
                return newImage;
            }
        }

        private async Task UploadFileToMinIoAsync(string bucketName, string objectName, string contentType,
            Stream stream)
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

            stream.Position = 0;
            // Upload a file to bucket
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithObjectSize(stream.Length)
                .WithStreamData(stream)
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

        private async Task DeleteFilesFromMinIoAsync(string bucketName, IList<string> objectNames)
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