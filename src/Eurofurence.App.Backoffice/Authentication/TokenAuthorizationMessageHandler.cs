using System.Security.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace Eurofurence.App.Backoffice.Authentication
{
    public class TokenAuthorizationMessageHandler : AuthorizationMessageHandler
    {
        public TokenAuthorizationMessageHandler(IAccessTokenProvider provider, NavigationManager navigation, IConfiguration configuration) : base(provider, navigation)
        {
            ConfigureHandler(new[] { configuration.GetValue<string>("ApiUrl") ?? string.Empty });
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                return await base.SendAsync(request, cancellationToken);
            }
            catch (AccessTokenNotAvailableException ex)
            {
                ex.Redirect();
                throw new AuthenticationException("Authentication has expired. Redirecting to login...");
            }
        }
    }
}
