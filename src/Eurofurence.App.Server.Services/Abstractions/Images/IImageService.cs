using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Images;

namespace Eurofurence.App.Server.Services.Abstractions.Images
{
    public interface IImageService : IEntityServiceOperations<ImageRecord, ImageResponse>
    {
        Task<ImageRecord> InsertImageAsync(
            string internalReference,
            Stream stream,
            bool isRestricted = false,
            int? width = null,
            int? height = null,
            CancellationToken cancellationToken = default);

        Task<ImageRecord> ReplaceImageAsync(
            Guid id,
            string internalReference,
            Stream stream,
            bool isRestricted = false,
            int? width = null,
            int? height = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieve a <c>MemoryStream</c> of the image for the given ID from storage.
        /// </summary>
        /// <param name="id">ID of the <c>ImageRecord</c></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Stream of image data or <c>null</c> if no <c>ImageRecord</c> with given ID exists.</returns>
        /// <exception cref="Minio.Exceptions.ObjectNotFoundException">
        /// Thrown when file associated to existing <c>ImageRecord</c> has unexpectedly been deleted
        /// from storage.
        /// </exception>
        Task<MemoryStream> GetImageStreamByImageIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieve contents of the image for the given ID from storage.
        /// </summary>
        /// <param name="id">ID of the <c>ImageRecord</c></param>
        /// <param name="cancellationToken"></param>
        /// <returns>byte array with image data or <c>null</c> if no <c>ImageRecord</c> with given ID exists.</returns>
        /// <exception cref="Minio.Exceptions.ObjectNotFoundException">
        /// Thrown when file associated to existing <c>ImageRecord</c> has unexpectedly been deleted
        /// from storage.
        /// </exception>
        Task<byte[]> GetImageContentByImageIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Stream GeneratePlaceholderImage();

        [Obsolete("Use InsertImageAsync or ReplaceImageAsync with dimensions instead.")]
        Task<ImageRecord> EnforceMaximumDimensionsAsync(
            ImageRecord image,
            int width,
            int height,
            CancellationToken cancellationToken = default);

        Task<ImageRecord> SetRestricted(Guid id, bool isRestricted,
            CancellationToken cancellationToken = default);
    }
}