using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.PushNotifications;
using Eurofurence.App.Infrastructure.EntityFramework;
using IdentityModel.AspNetCore.OAuth2Introspection;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Eurofurence.App.Server.Web.Identity;

public class RolesClaimsTransformation(
    IOptionsSnapshot<AuthorizationOptions> authorizationOptions,
    IOptionsMonitor<IdentityOptions> identityOptions,
    IHttpClientFactory httpClientFactory,
    IDistributedCache cache,
    AppDbContext db
) : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity)
        {
            return principal;
        }

        if (identity.FindFirst("token")?.Value is not { Length: > 0 } token)
        {
            return principal;
        }

        await ReadUserInfo(identity, token);

        await ReadRegSys(identity, token);

        var roles = new HashSet<string>();

        foreach (var claim in identity.Claims.Where(x => x.Type == "groups"))
        {
            if (authorizationOptions.Value.Admin.Contains(claim.Value))
            {
                roles.Add("Admin");
            }

            if (authorizationOptions.Value.System.Contains(claim.Value))
            {
                roles.Add("System");
            }

            if (authorizationOptions.Value.Developer.Contains(claim.Value))
            {
                roles.Add("Developer");
            }

            if (authorizationOptions.Value.KnowledgeBaseMaintainer.Contains(claim.Value))
            {
                roles.Add("KnowledgeBase-Maintainer");
            }

            if (authorizationOptions.Value.ArtShow.Contains(claim.Value))
            {
                roles.Add("ArtShow");
            }

            if (authorizationOptions.Value.FursuitBadgeSystem.Contains(claim.Value))
            {
                roles.Add("FursuitBadgeSystem");
            }

            if (authorizationOptions.Value.PrivateMessagesSend.Contains(claim.Value))
            {
                roles.Add("Action-PrivateMessages-Send");
            }

            if (authorizationOptions.Value.PrivateMessagesQuery.Contains(claim.Value))
            {
                roles.Add("Action-PrivateMessages-Query");
            }
        }

        foreach (var role in roles)
        {
            identity.AddClaim(new Claim(identity.RoleClaimType, role));
        }


        return principal;
    }

    private async Task ReadUserInfo(ClaimsIdentity identity, string token)
    {
        if (await cache.GetStringAsync($"{token}_userinfo") is { Length: > 0 } cached)
        {
            foreach (var claim in JsonSerializer.Deserialize<List<CachedClaim>>(cached))
            {
                identity.AddClaim(new Claim(claim.Type, claim.Value));
            }

            return;
        }

        var current = identityOptions.CurrentValue;
        using var client = httpClientFactory.CreateClient(OAuth2IntrospectionDefaults.BackChannelHttpClientName);

        var response = await client.GetUserInfoAsync(new UserInfoRequest
        {
            Address = current.UserInfoEndpoint,
            Token = token
        });

        identity.AddClaims(response.Claims);

        var exp = identity.FindFirst(x => x.Type == "exp");
        if (exp is not null && long.TryParse(exp.Value, out var seconds))
        {
            await cache.SetStringAsync(
                $"{token}_userinfo",
                JsonSerializer.Serialize(response.Claims.Select(x => new CachedClaim(x.Type, x.Value)).ToList()),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = DateTimeOffset.FromUnixTimeSeconds(seconds)
                }
            );
        }
    }

    private async Task ReadRegSys(ClaimsIdentity identity, string token)
    {
        if (await cache.GetStringAsync($"{token}_regsys") is { Length: > 0 } cached)
        {
            identity.AddClaim(new Claim(identity.RoleClaimType, "Attendee"));

            foreach (var value in JsonSerializer.Deserialize<List<string>>(cached))
            {
                identity.AddClaim(new Claim("RegSysId", value));
            }

            return;
        }

        var current = identityOptions.CurrentValue;
        using var client = httpClientFactory.CreateClient(OAuth2IntrospectionDefaults.BackChannelHttpClientName);

        var request = new HttpRequestMessage(HttpMethod.Get,
            new Uri(new Uri(current.RegSysUrl), "attsrv/api/rest/v1/attendees"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            return;
        }

        var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var ids = json.RootElement.TryGetStringArray("ids").ToList();

        if (ids.Count == 0)
        {
            return;
        }

        identity.AddClaim(new Claim(identity.RoleClaimType, "Attendee"));

        foreach (var id in ids)
        {
            identity.AddClaim(new Claim("RegSysId", id));
        }


        var exp = identity.FindFirst(x => x.Type == "exp");
        if (exp is not null && long.TryParse(exp.Value, out var seconds))
        {
            await cache.SetStringAsync(
                $"{token}_regsys",
                JsonSerializer.Serialize(ids),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = DateTimeOffset.FromUnixTimeSeconds(seconds)
                }
            );
        }
    }

    private async Task UpdateRegSysIdsInDb(List<string> ids, string identityId)
    {
        var newIds = new HashSet<string>(ids);

        var existingIds = await db.Devices
            .Where(x => newIds.Contains(x.RegSysId)).Select(x => x.RegSysId)
            .ToListAsync();

        foreach (var existingId in existingIds)
        {
            newIds.Remove(existingId);
        }

        if (newIds.Count > 0)
        {
            var devices = await db.Devices
                .Where(x => x.RegSysId == null && x.IdentityId == identityId)
                .ToListAsync();

            foreach (var id in newIds)
            {
                foreach (var device in devices)
                {
                    db.Devices.Add(new DeviceRecord
                    {
                        IdentityId = identityId,
                        RegSysId = id,
                        DeviceToken = device.DeviceToken,
                        IsAndroid = device.IsAndroid
                    });
                }
            }

            await db.SaveChangesAsync();
        }
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