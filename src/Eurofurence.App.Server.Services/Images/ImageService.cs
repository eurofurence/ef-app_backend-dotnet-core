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
using System.Threading;
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
        private readonly IMinioClient _minIoClient;
        private readonly MinIoConfiguration _minIoConfiguration;

        public ImageService(
            AppDbContext appDbContext,
            IStorageServiceFactory storageServiceFactory,
            MinIoConfiguration minIoConfiguration)
            : base(appDbContext, storageServiceFactory)
        {
            _appDbContext = appDbContext;
            _minIoConfiguration = minIoConfiguration;
            _minIoClient = new MinioClient().WithEndpoint(minIoConfiguration.Endpoint)
                .WithCredentials(minIoConfiguration.AccessKey, minIoConfiguration.SecretKey)
                .WithRegion(minIoConfiguration.Region)
                .WithSSL(minIoConfiguration.Secure)
                .Build();
        }

        public override Task ReplaceOneAsync(ImageRecord entity, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException();
        }

        public override Task InsertOneAsync(ImageRecord entity, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException();
        }

        public override async Task DeleteOneAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _appDbContext.Images
                .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

            await DeleteFileFromMinIoAsync(
                _minIoConfiguration.Bucket,
                entity.InternalFileName,
                cancellationToken
            );

            await base.DeleteOneAsync(id, cancellationToken);
        }

        public override async Task DeleteAllAsync(CancellationToken cancellationToken = default)
        {
            var imageIds = await _appDbContext.Images
                .Select(image => image.Id.ToString())
                .ToListAsync(cancellationToken);

            await DeleteFilesFromMinIoAsync(
                _minIoConfiguration.Bucket,
                imageIds,
                cancellationToken
            );

            await base.DeleteAllAsync(cancellationToken);
        }

        public async Task<ImageRecord> InsertImageAsync(
            string internalReference,
            Stream stream,
            bool isRestricted = false,
            int? width = null,
            int? height = null,
            CancellationToken cancellationToken = default)
        {
            await using MemoryStream ms = new(), resizedImageStream = new();
            if (width is { } w && height is { } h)
            {
                await ResizeToMaximumDimensionsAsync(stream, resizedImageStream, w, h, cancellationToken);
                stream = resizedImageStream;
            }

            stream.Position = 0;
            await stream.CopyToAsync(ms, cancellationToken);
            var byteArray = ms.ToArray();
            var hash = Hashing.ComputeHashSha1(byteArray);
            var image = Image.Load(byteArray);

            var imageFormat = image.Metadata.DecodedImageFormat;

            var guid = Guid.NewGuid();

            var fileName = imageFormat != null
                ? guid + "." + imageFormat.FileExtensions.FirstOrDefault("png")
                : guid + ".png";

            var record = new ImageRecord
            {
                Id = guid,
                InternalFileName = fileName,
                Url = $"{_minIoConfiguration.BaseUrl ?? _minIoClient.Config.Endpoint}/{_minIoConfiguration.Bucket}/{fileName}",
                IsRestricted = isRestricted,
                InternalReference = internalReference,
                IsDeleted = 0,
                MimeType = imageFormat?.DefaultMimeType,
                Width = image.Width,
                Height = image.Height,
                SizeInBytes = stream.Length,
                ContentHashSha1 = hash
            };

            await base.InsertOneAsync(record, cancellationToken);

            await UploadFileToMinIoAsync(
                _minIoConfiguration.Bucket,
                record.InternalFileName,
                imageFormat?.DefaultMimeType,
                stream,
                cancellationToken
            );

            await _appDbContext.SaveChangesAsync(cancellationToken);

            return record;
        }

        public async Task<ImageRecord> ReplaceImageAsync(
            Guid id,
            string internalReference,
            Stream stream,
            bool isRestricted = false,
            int? width = null,
            int? height = null,
            CancellationToken cancellationToken = default)
        {
            await using MemoryStream ms = new(), resizedImageStream = new();
            if (width is { } w && height is { } h)
            {
                await ResizeToMaximumDimensionsAsync(stream, resizedImageStream, w, h, cancellationToken);
                stream = resizedImageStream;
            }

            stream.Position = 0;
            await stream.CopyToAsync(ms, cancellationToken);
            var byteArray = ms.ToArray();
            var hash = Hashing.ComputeHashSha1(byteArray);
            var image = Image.Load(byteArray);

            var imageFormat = image.Metadata.DecodedImageFormat;

            var existingRecord = await
                _appDbContext.Images
                    .AsNoTracking()
                    .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

            if (existingRecord.ContentHashSha1 == hash)
            {
                return existingRecord;
            }

            await DeleteFileFromMinIoAsync(
                _minIoConfiguration.Bucket,
                existingRecord.InternalFileName,
                cancellationToken
            );

            var fileName = imageFormat != null
                ? existingRecord.Id + "." + imageFormat.FileExtensions.FirstOrDefault("png")
                : existingRecord.Id + ".png";

            existingRecord.InternalFileName = fileName;
            existingRecord.Url = $"{_minIoConfiguration.BaseUrl ?? _minIoClient.Config.Endpoint}/{_minIoConfiguration.Bucket}/{fileName}";
            existingRecord.IsRestricted = isRestricted;
            existingRecord.InternalReference = internalReference;
            existingRecord.MimeType = imageFormat?.DefaultMimeType;
            existingRecord.Width = image.Width;
            existingRecord.Height = image.Height;
            existingRecord.SizeInBytes = stream.Length;
            existingRecord.ContentHashSha1 = hash;

            await UploadFileToMinIoAsync(
                _minIoConfiguration.Bucket,
                existingRecord.InternalFileName,
                imageFormat?.DefaultMimeType,
                stream,
                cancellationToken
            );
            await base.ReplaceOneAsync(existingRecord, cancellationToken);
            return existingRecord;
        }

        public async Task<Stream> GetImageContentByImageIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var existingRecord = await
                _appDbContext.Images
                    .AsNoTracking()
                    .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

            // Checks if the object exists
            await _minIoClient.StatObjectAsync(new StatObjectArgs()
                    .WithBucket(_minIoConfiguration.Bucket)
                    .WithObject(existingRecord.InternalFileName),
                cancellationToken);

            var ms = new MemoryStream();
            await _minIoClient.GetObjectAsync(new GetObjectArgs()
                    .WithBucket(_minIoConfiguration.Bucket)
                    .WithObject(existingRecord.InternalFileName)
                    .WithCallbackStream(stream => { stream.CopyTo(ms); }),
                cancellationToken);

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

        public async Task<ImageRecord> EnforceMaximumDimensionsAsync(
            ImageRecord image,
            int width,
            int height,
            CancellationToken cancellationToken = default)
        {
            if (image == null) return null;

            var scaling = Math.Min((double)width / image.Width, (double)height / image.Height);
            if (scaling >= 1) return image;

            await using Stream resizedImageStream = new MemoryStream(),
                stream = await GetImageContentByImageIdAsync(image.Id, cancellationToken);
            await ResizeToMaximumDimensionsAsync(stream, resizedImageStream, height, width, cancellationToken);
            var newImage = await ReplaceImageAsync(
                image.Id,
                image.InternalReference,
                resizedImageStream,
                cancellationToken: cancellationToken
            );
            return newImage;
        }

        private static async Task ResizeToMaximumDimensionsAsync(Stream inputStream, Stream outputStream, int width, int height, CancellationToken cancellationToken = default)
        {
            if (inputStream == null || outputStream == null) return;

            Image rawImage;
            using (MemoryStream ms = new())
            {
                inputStream.Position = 0;
                await inputStream.CopyToAsync(ms, cancellationToken);
                var byteArray = ms.ToArray();
                rawImage = Image.Load(byteArray);
            }

            var scaling = Math.Min((double)width / rawImage.Width, (double)height / rawImage.Height);
            if (scaling >= 1)
            {
                inputStream.Position = 0;
                await inputStream.CopyToAsync(outputStream, cancellationToken);
                return;
            }

            rawImage.Mutate(ctx =>
                ctx.Resize(new ResizeOptions()
                {
                    Mode = ResizeMode.Max,
                    Size = new SixLabors.ImageSharp.Size(width, height),
                    Sampler = new BicubicResampler()
                })
            );

            await rawImage.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = 85 }, cancellationToken);
        }

        private async Task UploadFileToMinIoAsync(
            string bucketName,
            string objectName,
            string contentType,
            Stream stream,
            CancellationToken cancellationToken = default)
        {
            // Make a bucket on the server, if not already present
            var found = await _minIoClient
                .BucketExistsAsync(new BucketExistsArgs()
                        .WithBucket(bucketName),
                    cancellationToken)
                .ConfigureAwait(false);
            if (!found)
            {
                var mbArgs = new MakeBucketArgs()
                    .WithBucket(bucketName);
                await _minIoClient.MakeBucketAsync(mbArgs, cancellationToken).ConfigureAwait(false);
            }

            stream.Position = 0;
            // Upload a file to bucket
            var putObjectArgs = new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithObjectSize(stream.Length)
                .WithStreamData(stream)
                .WithContentType(contentType);
            await _minIoClient.PutObjectAsync(putObjectArgs, cancellationToken).ConfigureAwait(false);
        }

        private async Task DeleteFileFromMinIoAsync(
            string bucketName,
            string objectName,
            CancellationToken cancellationToken = default)
        {
            // If the bucket does not exist, return
            var found = await _minIoClient
                .BucketExistsAsync(new BucketExistsArgs()
                        .WithBucket(bucketName),
                    cancellationToken)
                .ConfigureAwait(false);
            if (!found)
            {
                return;
            }

            // Remove the file from bucket
            var removeObjectArgs = new RemoveObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName);
            await _minIoClient.RemoveObjectAsync(removeObjectArgs, cancellationToken).ConfigureAwait(false);
        }

        private async Task DeleteFilesFromMinIoAsync(
            string bucketName,
            IList<string> objectNames,
            CancellationToken cancellationToken = default)
        {
            // If the bucket does not exist, return
            var found = await _minIoClient
                .BucketExistsAsync(new BucketExistsArgs()
                        .WithBucket(bucketName),
                    cancellationToken)
                .ConfigureAwait(false);
            if (!found)
            {
                return;
            }

            // Remove the file from bucket
            var removeObjectsArgs = new RemoveObjectsArgs()
                .WithBucket(bucketName)
                .WithObjects(objectNames);
            await _minIoClient.RemoveObjectsAsync(removeObjectsArgs, cancellationToken).ConfigureAwait(false);
        }

        public async Task<ImageRecord> SetRestricted(Guid id, bool isRestricted,
            CancellationToken cancellationToken = default)
        {
            var record = await _appDbContext.Images.FirstOrDefaultAsync(i => i.Id == id);
            if (record == null)
            {
                record.IsRestricted = isRestricted;
                await _appDbContext.SaveChangesAsync(cancellationToken);
            }
            return record;
        }
    }
}