using System;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Images;

namespace Eurofurence.App.Server.Services.Abstractions.Images
{
    public interface IImageService : IEntityServiceOperations<ImageRecord>
    {
        Task<Guid> InsertOrUpdateImageAsync(string internalReference, byte[] imageBytes);
        Task<byte[]> GetImageContentByIdAsync(Guid id);
    }
}
