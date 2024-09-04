using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.PushNotifications;
using Eurofurence.App.Domain.Model.Users;
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
            if (authorizationOptions.Value.ArtShow.Contains(claim.Value))
            {
                roles.Add("ArtShow");
            }

            if (authorizationOptions.Value.PrivateMessageSender.Contains(claim.Value))
            {
                roles.Add("PrivateMessageSender");
            }

            if (authorizationOptions.Value.KnowledgeBaseEditor.Contains(claim.Value))
            {
                roles.Add("KnowledgeBaseEditor");
            }

            if (authorizationOptions.Value.MapEditor.Contains(claim.Value))
            {
                roles.Add("MapEditor");
            }

            if (authorizationOptions.Value.FursuitBadgeSystem.Contains(claim.Value))
            {
                roles.Add("FursuitBadgeSystem");
            }

            if (authorizationOptions.Value.Admin.Contains(claim.Value))
            {
                roles.Add("Admin");
            }

            if (authorizationOptions.Value.Attendee.Contains(claim.Value))
            {
                roles.Add("Attendee");
            }

            if (authorizationOptions.Value.AttendeeCheckedIn.Contains(claim.Value))
            {
                roles.Add("AttendeeCheckedIn");
            }

            if (authorizationOptions.Value.ArtistAlleyAdmin.Contains(claim.Value))
            {
                roles.Add("ArtistAlleyAdmin");
            }

            if (authorizationOptions.Value.ArtistAlleyModerator.Contains(claim.Value))
            {
                roles.Add("ArtistAlleyModerator");
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
        var current = identityOptions.CurrentValue;

        if (string.IsNullOrEmpty(current.RegSysUrl))
        {
            return;
        }

        if (await cache.GetStringAsync($"{token}_regsys") is { Length: > 0 } cached)
        {
            identity.AddClaim(new Claim(identity.RoleClaimType, "Attendee"));

            var cachedRegistrations = JsonSerializer.Deserialize<Dictionary<string, UserRegistrationStatus>>(cached);

            foreach (var registration in cachedRegistrations)
            {
                identity.AddClaim(new Claim(UserRegistrationClaims.Id, registration.Key));
                identity.AddClaim(new Claim(UserRegistrationClaims.Status(registration.Key), registration.Value.ToString()));
            }

            if (cachedRegistrations.Any(registrationStatus => registrationStatus.Value == UserRegistrationStatus.CheckedIn))
            {
                identity.AddClaim(new Claim(identity.RoleClaimType, "AttendeeCheckedIn"));
            }

            return;
        }

        using var client = httpClientFactory.CreateClient(OAuth2IntrospectionDefaults.BackChannelHttpClientName);

        var request = new HttpRequestMessage(HttpMethod.Get,
            new Uri(new Uri($"{current.RegSysUrl.TrimEnd('/')}/"), "attsrv/api/rest/v1/attendees"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var response = await client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            return;
        }

        var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var registrations = (await Task.WhenAll(
            json.RootElement.TryGetStringArray("ids").ToDictionary(id => id, async id =>
            {
                var status = await GetRegistrationStatus(current.RegSysUrl, token, id);

                identity.AddClaim(new Claim(UserRegistrationClaims.Id, id));
                identity.AddClaim(new Claim(UserRegistrationClaims.Status(id), status.ToString()));
                return status;
            }).Select(
                async registration => new { Id = registration.Key, Status = await registration.Value }
            )
        )).ToDictionary(registration => registration.Id, registration => registration.Status);

        if (registrations.Count == 0)
        {
            return;
        }

        identity.AddClaim(new Claim(identity.RoleClaimType, "Attendee"));

        if (registrations.Any(registration => registration.Value == UserRegistrationStatus.CheckedIn))
        {
            identity.AddClaim(new Claim(identity.RoleClaimType, "AttendeeCheckedIn"));
        }

        if (identity.FindFirst("sub")?.Value is { Length: > 0 } identityId)
        {
            await UpdateRegSysIdsInDb(registrations.Keys.ToList(), identityId);
        }

        var exp = identity.FindFirst(x => x.Type == "exp");
        if (exp is not null && long.TryParse(exp.Value, out var seconds))
        {
            await cache.SetStringAsync(
                $"{token}_regsys",
                JsonSerializer.Serialize(registrations),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = DateTimeOffset.FromUnixTimeSeconds(seconds)
                }
            );
        }
    }

    private async Task<UserRegistrationStatus> GetRegistrationStatus(string regSysUrl, string token, string id)
    {
        using var client = httpClientFactory.CreateClient(OAuth2IntrospectionDefaults.BackChannelHttpClientName);

        var statusRequest = new HttpRequestMessage(HttpMethod.Get,
                new Uri(new Uri($"{regSysUrl.TrimEnd('/')}/"), $"attsrv/api/rest/v1/attendees/{id}/status"));
        statusRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var statusResponse = await client.SendAsync(statusRequest);

        if (statusResponse.IsSuccessStatusCode)
        {
            var statusJson = await JsonDocument.ParseAsync(await statusResponse.Content.ReadAsStreamAsync());
            Enum.TryParse(statusJson.RootElement.TryGetString("status")?.Replace(" ", ""), true, out UserRegistrationStatus status);
            return status;
        }

        return UserRegistrationStatus.Unknown;
    }

    private async Task UpdateRegSysIdsInDb(List<string> ids, string identityId)
    {
        var newIds = new HashSet<string>(ids);

        var existingIds = await db.RegistrationIdentities
            .AsNoTracking()
            .Where(x => newIds.Contains(x.RegSysId))
            .Select(x => x.RegSysId)
            .ToListAsync();

        foreach (var existingId in existingIds)
        {
            newIds.Remove(existingId);
        }

        if (newIds.Count > 0)
        {
            await db.RegistrationIdentities.AddRangeAsync(newIds.Select(x => new RegistrationIdentityRecord
            {
                RegSysId = x,
                IdentityId = identityId
            }));

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