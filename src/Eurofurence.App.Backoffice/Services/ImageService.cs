using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Eurofurence.App.Domain.Model.Images;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Headers;
using static Google.Apis.Requests.BatchRequest;

namespace Eurofurence.App.Backoffice.Services
{
    public class ImageService(HttpClient http) : IImageService
    {
        public async Task<ImageRecord[]> GetImagesAsync()
        {
            return (await http.GetFromJsonAsync<ImageRecord[]>("images"))?.Where(ke => ke.IsDeleted != 1).ToArray() ?? [];
        }

        public async Task<string> GetImageContentAsync(Guid id)
        {
            try
            {
                return Convert.ToBase64String(await http.GetByteArrayAsync($"images/{id}/Content"));
            }
            catch
            {
                return string.Empty;
            }
        }

        public async Task<ImageRecord?> PutImageAsync(Guid id, IBrowserFile file)
        {
            using (var content = new MultipartFormDataContent())
            {
                var fileContent = new StreamContent(file.OpenReadStream(file.Size));
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                content.Add(content: fileContent, name: "file", fileName: file.Name);
                var response = await http.PutAsync($"images/{id}", content);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return null;
                }
                return JsonSerializer.Deserialize<ImageRecord>(await response.Content.ReadAsStreamAsync());
            }
        }

        public async Task<ImageRecord?> PostImageAsync(IBrowserFile file)
        {
            using (var content = new MultipartFormDataContent())
            {
                var fileContent = new StreamContent(file.OpenReadStream(file.Size));
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                content.Add(content: fileContent, name: "file", fileName: file.Name);
                var response = await http.PostAsync($"images", content);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return null;
                }
                return JsonSerializer.Deserialize<ImageRecord>(await response.Content.ReadAsStreamAsync());
            }
        }

        public async Task DeleteImageAsync(Guid id)
        {
            await http.DeleteAsync($"images/{id}");
        }
    }
}
