using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net.Http.Headers;

namespace Eurofurence.App.Backoffice.Client.Authentication
{
    public class ApiAuthorizationMessageHandler : DelegatingHandler
    {
        private readonly IAccessTokenProvider _accessTokenProvider;

        public ApiAuthorizationMessageHandler(IAccessTokenProvider accessTokenProvider)
        {
            _accessTokenProvider = accessTokenProvider;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var tokenResult = await _accessTokenProvider.RequestAccessToken();
            if (tokenResult.TryGetToken(out var accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Value);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}