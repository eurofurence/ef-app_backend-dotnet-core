using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Eurofurence.App.Server.Services.Abstractions.Security;

namespace Eurofurence.App.Server.Services.Security
{
    public class RegSysAuthenticationBridge : IRegSysAuthenticationBridge
    {
        public async Task<bool> VerifyCredentialSetAsync(int regNo, string username, string password)
        {
            using (var client = new HttpClient())
            {
                var payload = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("id", regNo.ToString()),
                    new KeyValuePair<string, string>("nick", username),
                    new KeyValuePair<string, string>("password", password),
                });

                var response = await client.PostAsync("https://reg.eurofurence.org/regsys/api/authcheck", payload);

                return response.IsSuccessStatusCode;               
            }
        }
    }
}
