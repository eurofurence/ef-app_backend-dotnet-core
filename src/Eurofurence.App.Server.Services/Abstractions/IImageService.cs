using Eurofurence.App.Domain.Model.Images;
using System;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public interface IImageService : IEntityServiceOperations<ImageRecord>
    {
        Task<Guid> InsertOrUpdateImageAsync(string internalReference, byte[] imageBytes);
        Task<byte[]> GetImageContentByIdAsync(Guid id);
    }
}
