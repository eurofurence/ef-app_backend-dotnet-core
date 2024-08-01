using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;
using Eurofurence.App.Domain.Model.ArtistsAlley;

namespace Eurofurence.App.Backoffice.Services
{
    public class ArtistAlleyService(HttpClient http) : IArtistAlleyService
    {
        public async Task<TableRegistrationResponse[]> GetTableRegistrationsAsync()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());
            return (await http.GetFromJsonAsync<TableRegistrationResponse[]>("ArtistsAlley", options))?.Where(ke => ke.IsDeleted != 1).ToArray() ?? [];
        }
    }
}
