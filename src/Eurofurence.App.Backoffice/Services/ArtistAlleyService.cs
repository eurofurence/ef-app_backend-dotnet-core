using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;
using Eurofurence.App.Domain.Model.ArtistsAlley;

namespace Eurofurence.App.Backoffice.Services
{
    public class ArtistAlleyService(HttpClient http) : IArtistAlleyService
    {
        public async Task<TableRegistrationRecord[]> GetTableRegistrationsAsync()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());
            return (await http.GetFromJsonAsync<TableRegistrationRecord[]>("ArtistsAlley", options))?.Where(ke => ke.IsDeleted != 1).ToArray() ?? [];
        }

        public async Task PutTableRegistrationStatusAsync(TableRegistrationRecord record,
            TableRegistrationRecord.RegistrationStateEnum state)
        {
            JsonContent content = JsonContent.Create(state);
            HttpResponseMessage res = await http.PutAsync($"ArtistsAlley/{record.Id}/:status", content);
        }

        public async Task DeleteTableRegistrationAsync(TableRegistrationRecord record)
        {
            await http.DeleteAsync($"ArtistsAlley/{record.Id}");
        }
        public async Task PutArtistAllaySystemStatus(ArtistAlleySystemStatus status)
        {
            JsonContent content = JsonContent.Create(status);
            await http.PutAsync("ArtistsAlley/SystemStatus", content);
        }
        public async Task<ArtistAlleySystemStatus> GetArtistAllaySystemStatus()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());
            return await http.GetFromJsonAsync<ArtistAlleySystemStatus>("ArtistsAlley/SystemStatus",options);
        }
    }
}
