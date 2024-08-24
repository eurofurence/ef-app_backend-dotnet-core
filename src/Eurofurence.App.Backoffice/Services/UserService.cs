using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;
using Eurofurence.App.Domain.Model.Users;

namespace Eurofurence.App.Backoffice.Services
{
    public class UserService(HttpClient http) : IUserService
    {
        public async Task<UserRecord> GetUserSelf()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());
            return await http.GetFromJsonAsync<UserRecord>("Users/:self", options) ?? new UserRecord();
        }
    }
}