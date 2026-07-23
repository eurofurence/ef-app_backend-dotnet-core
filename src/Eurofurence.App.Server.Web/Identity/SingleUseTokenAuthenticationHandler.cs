using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Eurofurence.App.Server.Web.Identity;

public class SingleUseTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IDistributedCache _cache;

    public SingleUseTokenAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, IDistributedCache cache, UrlEncoder encoder) : base(options, logger, encoder)
    {
        _cache = cache;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var requestToken = Context.Request.Query[SingleUseTokenAuthenticationDefaults.QueryName].FirstOrDefault();

        if (string.IsNullOrEmpty(requestToken))
        {
            return AuthenticateResult.Fail("Missing token.");
        }

        Logger.LogDebug("Attempting single-use token authentication…");

        if (await _cache.GetStringAsync(requestToken) is var rawToken
            && !string.IsNullOrEmpty(rawToken))
        {
            await _cache.RemoveAsync(requestToken);

            var token = JsonSerializer.Deserialize<SingleUseToken>(rawToken);

            if (DateTimeOffset.UtcNow.CompareTo(token.ValidUntil) > 0)
            {
                return AuthenticateResult.Fail("Expired token.");
            }

            Logger.LogInformation($"Matched token for {token.PrincipalName} with roles {string.Join(',', token.Roles)} valid until {token.ValidUntil:u}.");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, token.PrincipalName),
                new Claim(ClaimTypes.Expiration, token.ValidUntil.ToUnixTimeSeconds().ToString()),
            };

            foreach (var role in token.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            foreach (var claim in token.AdditionalClaims)
            {
                claims.Add(new Claim(claim.Key, claim.Value));
            }

            var identity = new ClaimsIdentity(claims, SingleUseTokenAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return AuthenticateResult.Success(ticket);
        }

        return AuthenticateResult.Fail("Invalid token.");
    }
}