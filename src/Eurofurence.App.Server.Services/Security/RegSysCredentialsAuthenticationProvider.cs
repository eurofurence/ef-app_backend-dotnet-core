using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Eurofurence.App.Server.Services.Abstractions.Security;

namespace Eurofurence.App.Server.Services.Security
{
    public class RegSysCredentialsAuthenticationProvider : IAuthenticationProvider
    {
        private async Task<bool> VerifyCredentialSetAsync(int regNo, string username, string password)
        {
            using (var client = new HttpClient())
            {
                var payload = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("id", regNo.ToString()),
                    new KeyValuePair<string, string>("nick", username),
                    new KeyValuePair<string, string>("password", password)
                });

                payload.Headers.Remove("Content-Type");
                payload.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded;charset=UTF-8");

                var response = await client.PostAsync("https://reg.eurofurence.org/regsys/api/authcheck", payload);

                return response.IsSuccessStatusCode;
            }
        }

        public async Task<AuthenticationResult> ValidateRegSysAuthenticationRequestAsync(RegSysAuthenticationRequest request)
        {
            bool isValid = await VerifyCredentialSetAsync(request.RegNo, request.Username, request.Password);

            return isValid
                ? new AuthenticationResult
                {
                    IsAuthenticated = true,
                    RegNo = request.RegNo,
                    Username = request.Username,
                    Source = GetType().Name
                }
                : new AuthenticationResult
                {
                    IsAuthenticated = false,
                    Source = GetType().Name
                };
        }
    }
}