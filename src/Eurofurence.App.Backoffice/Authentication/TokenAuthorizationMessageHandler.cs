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
    }
}
