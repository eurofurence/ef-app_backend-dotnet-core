using Eurofurence.App.Domain.Model.Images;
using Microsoft.AspNetCore.Components.Forms;

namespace Eurofurence.App.Backoffice.Services
{
    public interface IImageService
    {
        public Task<ImageResponse?> GetImageAsync(Guid id);
        public Task<ImageResponse[]> GetImagesAsync();
        public Task<ImageWithRelationsResponse[]> GetImagesWithRelationsAsync();
        public Task<ImageResponse?> PutImageAsync(Guid id, IBrowserFile file);
        public Task<ImageResponse?> PostImageAsync(IBrowserFile file);
        public Task<bool> DeleteImageAsync(Guid id);
    }
}
