using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Eurofurence.App.Server.Web.Identity;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    public ApiKeyAuthenticationHandler(IOptionsMonitor<ApiKeyAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var requestApiKey = Context.Request.Headers[ApiKeyAuthenticationDefaults.HeaderName].FirstOrDefault();

        if (Options.ApiKeys is null || Options.ApiKeys.Count == 0 || requestApiKey is null) return Task.FromResult(AuthenticateResult.Fail("Invalid X-API-Key."));

        Logger.LogDebug("Attempting API key authentication…");

        if (Options.ApiKeys.FirstOrDefault(apiKey => apiKey.Key == requestApiKey && DateTime.Now.CompareTo(apiKey.ValidUntil) <= 0) is { } apiKeyOptions)
        {
            Logger.LogInformation($"Matched API key for {apiKeyOptions.PrincipalName} with roles {string.Join(',', apiKeyOptions.Roles)} valid until {apiKeyOptions.ValidUntil}.");

            var claims = new List<Claim>
            {
                new Claim("name", apiKeyOptions.PrincipalName),
                new Claim("sub", $"{ApiKeyAuthenticationDefaults.AuthenticationScheme}:{apiKeyOptions.PrincipalName}"),
                new Claim(ClaimTypes.Role, ApiKeyAuthenticationDefaults.AuthenticationScheme)
            };

            foreach (var role in apiKeyOptions.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        return Task.FromResult(AuthenticateResult.Fail("Invalid X-API-Key."));
    }
}