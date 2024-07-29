using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;
using Eurofurence.App.Domain.Model.Fursuits;

namespace Eurofurence.App.Backoffice.Services
{
    public class FursuitService(HttpClient http) : IFursuitService
    {
        public async Task<FursuitBadgeResponse[]> GetFursuitBadgesAsync()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());
            return (await http.GetFromJsonAsync<FursuitBadgeResponse[]>("Fursuits/Badges", options))?.Where(ke => ke.IsDeleted != 1).ToArray() ?? [];
        }
    }
}
