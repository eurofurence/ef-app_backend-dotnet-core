using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel.AspNetCore.OAuth2Introspection;
using Microsoft.Extensions.Options;

namespace Eurofurence.App.Server.Web.Identity;

public class ConfigureOAuth2IntrospectionOptions(
    IOptionsMonitor<IdentityOptions> identityOptions
) : IConfigureNamedOptions<OAuth2IntrospectionOptions>
{
    public void Configure(OAuth2IntrospectionOptions options)
    {
        var current = identityOptions.CurrentValue;
        options.IntrospectionEndpoint = current.IntrospectionEndpoint;
        options.ClientId = current.ClientId;
        options.Events.OnTokenValidated = OnTokenValidated;
    }

    public void Configure(string name, OAuth2IntrospectionOptions options)
    {
        Configure(options);
    }

    private async Task OnTokenValidated(TokenValidatedContext context)
    {
        if (context.Principal?.Identity is ClaimsIdentity identity)
        {
            identity.AddClaim(new Claim("token", context.SecurityToken));
        }
    }
}