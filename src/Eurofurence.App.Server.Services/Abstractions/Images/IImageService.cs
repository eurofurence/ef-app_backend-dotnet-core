using System;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Fragments;
using Eurofurence.App.Domain.Model.Images;

namespace Eurofurence.App.Server.Services.Abstractions.Images
{
    public interface IImageService : IEntityServiceOperations<ImageRecord>
    {
        Task InsertImageAsync(ImageRecord image, byte[] imageBytes);
        Task<ImageRecord> InsertOrUpdateImageAsync(string internalReference, byte[] imageBytes);
        Task<byte[]> GetImageContentByImageIdAsync(Guid id);
        byte[] GeneratePlaceholderImage();
        Task<ImageRecord> EnforceMaximumDimensionsAsync(ImageRecord image, int width, int height);
    }
}