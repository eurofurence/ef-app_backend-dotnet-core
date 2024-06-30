using System;
using System.IO;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Images;

namespace Eurofurence.App.Server.Services.Abstractions.Images
{
    public interface IImageService : IEntityServiceOperations<ImageRecord>
    {
        Task<ImageRecord> InsertImageAsync(string internalReference, Stream stream);
        Task<ImageRecord> ReplaceImageAsync(Guid id, string internalReference, Stream stream);
        Task<Stream> GetImageContentByImageIdAsync(Guid id);
        Stream GeneratePlaceholderImage();
        Task<ImageRecord> EnforceMaximumDimensionsAsync(ImageRecord image, int width, int height);
    }
}