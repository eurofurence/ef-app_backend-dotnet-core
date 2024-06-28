using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.AspNetCore.OAuth2Introspection;
using IdentityModel.Client;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Eurofurence.App.Server.Web.Identity;

public class ConfigureOAuth2IntrospectionOptions(
    IOptionsMonitor<IdentityOptions> identityOptions,
    IHttpClientFactory httpClientFactory,
    IDistributedCache cache
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
        var identity = (ClaimsIdentity)context.Principal.Identity;

        if (await TryReadCachedClaims(identity, context.SecurityToken, context.HttpContext.RequestAborted))
        {
            return;
        }

        var current = identityOptions.CurrentValue;
        using var client = httpClientFactory.CreateClient(OAuth2IntrospectionDefaults.BackChannelHttpClientName);

        var response = await client.GetUserInfoAsync(new UserInfoRequest
        {
            Address = current.UserInfoEndpoint,
            Token = context.SecurityToken
        }, context.HttpContext.RequestAborted);

        identity.AddClaims(response.Claims);

        var exp = identity.FindFirst(x => x.Type == "exp");
        if (exp is not null && long.TryParse(exp.Value, out var seconds))
        {
            await cache.SetStringAsync(
                $"{context.SecurityToken}_userinfo",
                JsonSerializer.Serialize(response.Claims.Select(x => new CachedClaim(x.Type, x.Value)).ToList()),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = DateTimeOffset.FromUnixTimeSeconds(seconds)
                }
            );
        }
    }

    private async Task<bool> TryReadCachedClaims(
        ClaimsIdentity identity,
        string token,
        CancellationToken cancellationToken = default)
    {
        var cached = await cache.GetStringAsync($"{token}_userinfo", cancellationToken);
        if (cached is null)
        {
            return false;
        }

        foreach (var claim in JsonSerializer.Deserialize<List<CachedClaim>>(cached))
        {
            identity.AddClaim(new Claim(claim.Type, claim.Value));
        }

        return true;
    }

    private class CachedClaim
    {
        public string Type { get; set; }

        public string Value { get; set; }

        public CachedClaim()
        {
        }

        public CachedClaim(string type, string value)
        {
            Type = type;
            Value = value;
        }
    }
}