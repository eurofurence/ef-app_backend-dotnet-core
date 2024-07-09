using Eurofurence.App.Domain.Model.Images;
using Microsoft.AspNetCore.Components.Forms;

namespace Eurofurence.App.Backoffice.Services
{
    public interface IImageService
    {
        public Task<ImageRecord[]> GetImagesAsync();
        public Task<string> GetImageContentAsync(Guid id);
        public Task<ImageRecord?> PutImageAsync(Guid id, IBrowserFile file);
        public Task<ImageRecord?> PostImageAsync(IBrowserFile file);
        public Task DeleteImageAsync(Guid id);
    }
}
