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

        Logger.LogInformation("Attempting API key authentication…");

        if (requestApiKey is null) return Task.FromResult(AuthenticateResult.Fail("No API key provided."));

        Logger.LogInformation($"Found API key: {requestApiKey}");

        if (Options.ApiKeys.FirstOrDefault(apiKey => apiKey.Key == requestApiKey && DateTime.Now.CompareTo(apiKey.ValidUntil) <= 0) is { } apiKeyOptions)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, apiKeyOptions.PrincipalName)
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