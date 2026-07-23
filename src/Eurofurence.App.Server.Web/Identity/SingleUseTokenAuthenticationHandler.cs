using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Eurofurence.App.Server.Services.Abstractions.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Eurofurence.App.Server.Web.Identity;

public class SingleUseTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly ISingleUseTokenService _tokenService;

    public SingleUseTokenAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        ISingleUseTokenService tokenService,
        UrlEncoder encoder) : base(options, logger, encoder)
    {
        _tokenService = tokenService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var requestToken = Context.Request.Query[SingleUseTokenAuthenticationDefaults.QueryName].FirstOrDefault();

        if (string.IsNullOrEmpty(requestToken))
        {
            return AuthenticateResult.Fail("Missing token.");
        }

        Logger.LogDebug("Attempting single-use token authentication…");

        var tokenPayload = _tokenService.TakeTokenPayload(requestToken);

        if (tokenPayload is null)
        {
            return AuthenticateResult.Fail("Invalid or expired token.");
        }

        Logger.LogInformation($"Matched token for {tokenPayload.PrincipalName} with roles {string.Join(',', tokenPayload.Roles)} valid until {tokenPayload.ValidUntil:u}.");

        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, tokenPayload.PrincipalName),
                new Claim(ClaimTypes.Expiration, tokenPayload.ValidUntil.ToUnixTimeSeconds().ToString()),
            };

        foreach (var role in tokenPayload.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        foreach (var claim in tokenPayload.AdditionalClaims)
        {
            claims.Add(new Claim(claim.Key, claim.Value));
        }

        var identity = new ClaimsIdentity(claims, SingleUseTokenAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return AuthenticateResult.Success(ticket);
    }
}