using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;
using Eurofurence.App.Domain.Model.Dealers;

namespace Eurofurence.App.Backoffice.Services
{
    public class DealerService(HttpClient http) : IDealerService
    {
        public async Task<DealerRecord[]> GetDealersAsync()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());
            return (await http.GetFromJsonAsync<DealerRecord[]>("Dealers", options))?.Where(ke => ke.IsDeleted != 1).ToArray() ?? [];
        }
    }
}
