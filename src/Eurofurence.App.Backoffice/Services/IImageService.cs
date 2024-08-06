using Eurofurence.App.Domain.Model.Images;
using Microsoft.AspNetCore.Components.Forms;

namespace Eurofurence.App.Backoffice.Services
{
    public interface IImageService
    {
        public Task<ImageRecord?> GetImageAsync(Guid id);
        public Task<ImageRecord[]> GetImagesAsync();
        public Task<ImageWithRelationsResponse[]> GetImagesWithRelationsAsync();
        public Task<ImageRecord?> PutImageAsync(Guid id, IBrowserFile file);
        public Task<ImageRecord?> PostImageAsync(IBrowserFile file);
        public Task<bool> DeleteImageAsync(Guid id);
    }
}
