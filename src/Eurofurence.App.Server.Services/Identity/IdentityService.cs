using Microsoft.Extensions.Caching.Distributed;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Users;
using Eurofurence.App.Domain.Model.PushNotifications;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions.Identity;
using Microsoft.EntityFrameworkCore;
using IdentityModel.AspNetCore.OAuth2Introspection;
using IdentityModel.Client;
using Microsoft.Extensions.Options;

namespace Eurofurence.App.Server.Services.Identity
{
    public class IdentityService : IIdentityService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IOptionsMonitor<IdentityOptions> _identityOptionsMonitor;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDistributedCache _cache;

        public IdentityService(
            AppDbContext appDbContext,
            IOptionsMonitor<IdentityOptions> identityOptions,
            IHttpClientFactory httpClientFactory,
            IDistributedCache cache)
        {
            _appDbContext = appDbContext;
            _identityOptionsMonitor = identityOptions;
            _httpClientFactory = httpClientFactory;
            _cache = cache;
        }

        public async Task ReadUserInfo(ClaimsIdentity identity)
        {
            if (identity.FindFirst("token")?.Value is not { Length: > 0 } token)
            {
                return;
            }

            if (await _cache.GetStringAsync($"{token}_userinfo") is { Length: > 0 } cached)
            {
                foreach (var claim in JsonSerializer.Deserialize<List<CachedClaim>>(cached))
                {
                    identity.AddClaim(new Claim(claim.Type, claim.Value));
                }

                return;
            }

            using var client = _httpClientFactory.CreateClient(OAuth2IntrospectionDefaults.BackChannelHttpClientName);

            var response = await client.GetUserInfoAsync(new UserInfoRequest
            {
                Address = _identityOptionsMonitor.CurrentValue.UserInfoEndpoint,
                Token = token
            });

            identity.AddClaims(response.Claims);

            var exp = identity.FindFirst(x => x.Type == "exp");
            if (exp is not null && long.TryParse(exp.Value, out var seconds))
            {
                await _cache.SetStringAsync(
                    $"{token}_userinfo",
                    JsonSerializer.Serialize(response.Claims.Select(x => new CachedClaim(x.Type, x.Value)).ToList()),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpiration = DateTimeOffset.FromUnixTimeSeconds(seconds)
                    }
                );
            }
        }

        public async Task ReadRegSys(ClaimsIdentity identity)
        {
            if (identity.FindFirst("token")?.Value is not { Length: > 0 } token)
            {
                return;
            }

            if (string.IsNullOrEmpty(_identityOptionsMonitor.CurrentValue.RegSysUrl))
            {
                return;
            }

            if (await _cache.GetStringAsync($"{token}_regsys") is { Length: > 0 } cached)
            {
                identity.AddClaim(new Claim(identity.RoleClaimType, "Attendee"));

                var cachedRegistrations = JsonSerializer.Deserialize<Dictionary<string, UserRegistrationStatus>>(cached);

                foreach (var registration in cachedRegistrations)
                {
                    identity.AddClaim(new Claim(UserRegistrationClaims.Id, registration.Key));
                    identity.AddClaim(new Claim(UserRegistrationClaims.Status(registration.Key),
                        registration.Value.ToString()));
                }

                if (cachedRegistrations.Any(registrationStatus =>
                        registrationStatus.Value == UserRegistrationStatus.CheckedIn))
                {
                    identity.AddClaim(new Claim(identity.RoleClaimType, "AttendeeCheckedIn"));
                }

                return;
            }

            using var client = _httpClientFactory.CreateClient(OAuth2IntrospectionDefaults.BackChannelHttpClientName);

            var request = new HttpRequestMessage(HttpMethod.Get,
                new Uri(new Uri($"{_identityOptionsMonitor.CurrentValue.RegSysUrl.TrimEnd('/')}/"), "attsrv/api/rest/v1/attendees"));
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
                    var status = await GetRegistrationStatus(token, id);

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

            if (identity.FindFirst("sub")?.Value is { Length: > 0 } identityId &&
                identity.FindFirst("name")?.Value is { Length: > 0 } nickname)
            {
                await UpdateRegSysIdsInDb(registrations.Keys.ToList(), identityId, nickname);
            }

            var exp = identity.FindFirst(x => x.Type == "exp");
            if (exp is not null && long.TryParse(exp.Value, out var seconds))
            {
                await _cache.SetStringAsync(
                    $"{token}_regsys",
                    JsonSerializer.Serialize(registrations),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpiration = DateTimeOffset.FromUnixTimeSeconds(seconds)
                    }
                );
            }
        }

        public async Task<IEnumerable<string>> GetRoleMembers(ClaimsIdentity identity, string role)
        {
            if (identity.FindFirst("token")?.Value is not { Length: > 0 } token)
            {
                return [];
            }

            using var client = _httpClientFactory.CreateClient(OAuth2IntrospectionDefaults.BackChannelHttpClientName);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var uri = new Uri(System.IO.Path.Combine(_identityOptionsMonitor.CurrentValue.GroupsEndpoint,
                $"{role}/users"));
            using var response = await client.GetAsync(uri);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GroupMembersResponse>();

            return result.Data.Select(d => d.UserId);
        }

        private async Task<UserRegistrationStatus> GetRegistrationStatus(string token, string id)
        {
            using var client = _httpClientFactory.CreateClient(OAuth2IntrospectionDefaults.BackChannelHttpClientName);

            var statusRequest = new HttpRequestMessage(HttpMethod.Get,
                new Uri(new Uri($"{_identityOptionsMonitor.CurrentValue.RegSysUrl.TrimEnd('/')}/"), $"attsrv/api/rest/v1/attendees/{id}/status"));
            statusRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using var statusResponse = await client.SendAsync(statusRequest);

            if (!statusResponse.IsSuccessStatusCode)
            {
                return UserRegistrationStatus.Unknown;
            }

            var statusJson = await JsonDocument.ParseAsync(await statusResponse.Content.ReadAsStreamAsync());
            Enum.TryParse(statusJson.RootElement.TryGetString("status")?.Replace(" ", ""), true,
                out UserRegistrationStatus status);
            return status;

        }

        private async Task UpdateRegSysIdsInDb(List<string> ids, string identityId, string nickname)
        {
            var newIds = new HashSet<string>(ids);

            var existingIds = await _appDbContext.Users
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
                await _appDbContext.Users.AddRangeAsync(newIds.Select(x => new UserRecord
                {
                    RegSysId = x,
                    IdentityId = identityId,
                    Nickname = nickname
                }));

                await _appDbContext.SaveChangesAsync();
            }
        }

        private sealed class CachedClaim
        {
            public string Type { get; set; }

            public string Value { get; set; }

            public CachedClaim(string type, string value)
            {
                Type = type;
                Value = value;
            }
        }

        private sealed class GroupMembersResponse : ProtocolResponse
        {
            [JsonPropertyName("data")]
            public GroupMemberResponseData[] Data { get; set; }
        }

        private sealed class GroupMemberResponseData
        {
            [JsonPropertyName("group_id")]
            public string GroupId { get; set; }

            [JsonPropertyName("user_id")]
            public string UserId { get; set; }

            [JsonPropertyName("level")]
            public string Level { get; set; }
        }
    }
}