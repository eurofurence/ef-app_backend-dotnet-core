using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Images;

namespace Eurofurence.App.Server.Services.Abstractions.Images
{
    public interface IImageService : IEntityServiceOperations<ImageRecord>
    {
        Task<ImageRecord> InsertImageAsync(
            string internalReference,
            Stream stream,
            CancellationToken cancellationToken = default);

        Task<ImageRecord> ReplaceImageAsync(
            Guid id,
            string internalReference,
            Stream stream,
            CancellationToken cancellationToken = default);

        Task<Stream> GetImageContentByImageIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Stream GeneratePlaceholderImage();

        Task<ImageRecord> EnforceMaximumDimensionsAsync(
            ImageRecord image,
            int width,
            int height,
            CancellationToken cancellationToken = default);
    }
}