using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Eurofurence.App.Domain.Model.Images;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Headers;

namespace Eurofurence.App.Backoffice.Services
{
    public class ImageService(HttpClient http) : IImageService
    {
        public async Task<ImageResponse?> GetImageAsync(Guid id)
        {
            return (await http.GetFromJsonAsync<ImageResponse>($"images/{id}"));
        }

        public async Task<ImageResponse[]> GetImagesAsync()
        {
            return (await http.GetFromJsonAsync<ImageResponse[]>("images/:all"))?.ToArray() ?? [];
        }

        public async Task<ImageWithRelationsResponse[]> GetImagesWithRelationsAsync()
        {
            return (await http.GetFromJsonAsync<ImageWithRelationsResponse[]>("images/with-relations"))?.Where(ke => ke.IsDeleted != 1).ToArray() ?? [];
        }

        public async Task<ImageResponse?> PutImageAsync(Guid id, IBrowserFile file)
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
                return JsonSerializer.Deserialize<ImageResponse>(await response.Content.ReadAsStreamAsync());
            }
        }

        public async Task<ImageResponse?> PostImageAsync(IBrowserFile file)
        {
            using (var content = new MultipartFormDataContent())
            {
                var fileContent = new StreamContent(file.OpenReadStream(file.Size));
                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                content.Add(content: fileContent, name: "file", fileName: file.Name);
                var response = await http.PostAsync($"images", content);
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }
                return JsonSerializer.Deserialize<ImageResponse>(await response.Content.ReadAsStreamAsync());
            }
        }

        public async Task<bool> DeleteImageAsync(Guid id)
        {
            var response = await http.DeleteAsync($"images/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
