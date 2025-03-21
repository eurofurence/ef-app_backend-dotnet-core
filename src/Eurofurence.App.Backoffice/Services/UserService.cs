using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;
using Eurofurence.App.Domain.Model.ArtistsAlley;
using Eurofurence.App.Domain.Model.Users;

namespace Eurofurence.App.Backoffice.Services
{
    public class UserService(HttpClient http) : IUserService
    {
        public async Task<UserResponse> GetUserSelf()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());
            return await http.GetFromJsonAsync<UserResponse>("Users/:self", options) ?? new UserResponse();
        }

        public async Task PutUserArtistAlleyStatusAsync(String userID, ArtistAlleyUserPenaltyChangeRequest changeRequest)
        {
            JsonContent content = JsonContent.Create(changeRequest);

            await http.PutAsync($"Users/{userID}/:artist_alley_penalty", content);
        }
    }
}