#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Duende.AspNetCore.Authentication.OAuth2Introspection;
using Duende.IdentityModel.Client;
using Eurofurence.App.Domain.Model.Announcements;
using Eurofurence.App.Domain.Model.Identity;
using Eurofurence.App.Domain.Model.PushNotifications;
using Eurofurence.App.Domain.Model.Users;
using Eurofurence.App.Infrastructure.EntityFramework;
using Eurofurence.App.Server.Services.Abstractions.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sentry;

namespace Eurofurence.App.Server.Services.Identity
{
    public class IdentityService : IIdentityService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IOptionsMonitor<IdentityOptions> _identityOptionsMonitor;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDistributedCache _cache;
        private readonly ILogger _logger;

        /// <summary>
        /// The name of the claim type for IDP groups.
        /// </summary>
        private const string IdpGroupClaimType = "groups";

        public IdentityService(
            AppDbContext appDbContext,
            IOptionsMonitor<IdentityOptions> identityOptions,
            IHttpClientFactory httpClientFactory,
            IDistributedCache cache,
            ILoggerFactory loggerFactory)
        {
            _appDbContext = appDbContext;
            _identityOptionsMonitor = identityOptions;
            _httpClientFactory = httpClientFactory;
            _cache = cache;
            _logger = loggerFactory.CreateLogger(GetType());
        }

        public async Task ReadUserInfo(ClaimsIdentity identity)
        {
            if (identity.FindFirst("token")?.Value is not { Length: > 0 } token)
            {
                return;
            }

            if (await _cache.GetStringAsync($"{token}_userinfo") is { Length: > 0 } cached &&
                JsonSerializer.Deserialize<List<CachedClaim>>(cached) is { Count: > 0 } claims)
            {
                foreach (var claim in claims)
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
            var identityId = identity.FindFirst("sub")?.Value;

            // FIX: IDP will occasionally omit name claim on userinfo; can be fixed by retrying later.
            //      This only rarely happens (every few hundred requests) so we can simply not cache
            //      the broken response and try again next time.
            var hasMissingNameBug = string.IsNullOrEmpty(
                response.Claims.FirstOrDefault(claim => claim.Type == "name")?.Value
            );
            if (hasMissingNameBug)
            {
                SentrySdk.CaptureMessage("IDP response to userinfo request missing 'name' claim.", SentryLevel.Warning);
                _logger.LogWarning("Response to userinfo request missing 'name' claim will not be cached.");
            }

            var exp = identity.FindFirst(x => x.Type == "exp");
            if (!hasMissingNameBug && exp is not null && long.TryParse(exp.Value, out var seconds))
            {
                await _cache.SetStringAsync(
                    $"{token}_userinfo",
                    JsonSerializer.Serialize(response.Claims.Select(x => new CachedClaim(x.Type, x.Value)).ToList()),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpiration = DateTimeOffset.FromUnixTimeSeconds(seconds)
                    }
                );

                if (!string.IsNullOrEmpty(identityId) &&
                    GetUserGroups(identity).ToArray() is { Length: > 0 } groups)
                {
                    var identityAnnouncementGroups = await _appDbContext.IdentityAnnouncementGroups.AsTracking().FirstOrDefaultAsync(iag => iag.IdentityId == identityId);

                    if (identityAnnouncementGroups is null)
                    {
                        _appDbContext.IdentityAnnouncementGroups.Add(new IdentityAnnouncementGroupsRecord
                        {
                            IdentityId = identityId,
                            Groups = groups
                        });
                    }
                    else
                    {
                        identityAnnouncementGroups.Groups = groups;
                        identityAnnouncementGroups.Touch();
                    }
                    await _appDbContext.SaveChangesAsync();
                }

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
                if (JsonSerializer.Deserialize<RegistrationData>(cached) is { } cachedRegistrationData)
                {
                    AddRegistrationToClaims(identity, cachedRegistrationData);
                    return;
                }
                else
                {
                    // Prune invalid cache item
                    await _cache.RemoveAsync($"{token}_regsys");
                }
            }

            try
            {
                var registrationId = await GetRegistrationId(token);
                var registrationData = await GetRegistrationStatus(token, registrationId);

                AddRegistrationToClaims(identity, registrationData);
                await UpdateRegistrationInDatabase(registrationData, identity);

                var exp = identity.FindFirst(x => x.Type == "exp");
                if (exp is not null && long.TryParse(exp.Value, out var expiresAt))
                {
                    await _cache.SetStringAsync(
                        $"{token}_regsys",
                        JsonSerializer.Serialize(registrationData),
                        new DistributedCacheEntryOptions
                        {
                            AbsoluteExpiration = DateTimeOffset.FromUnixTimeSeconds(expiresAt)
                        }
                    );
                }
            }
            catch (RegistrationSystemClientException ex)
            {
                SentrySdk.CaptureException(ex);
                _logger.LogWarning(ex, "Registration data not persisted due to failed read from regsys.");
            }
            catch (Exception ex)
            {
                SentrySdk.CaptureException(ex);
                _logger.LogError(ex, "Unexpected error when processing registration data for user from regsys.");
            }
        }

        public IEnumerable<string> GetUserGroups(ClaimsIdentity identity)
        {
            return identity.Claims
                .Where(claim => claim.Type == IdpGroupClaimType)
                .Select(claim => claim.Value);
        }

        public async Task<IEnumerable<string>> GetGroupMembers(string groupId)
        {
            if (_identityOptionsMonitor.CurrentValue.GroupReaderToken is not { Length: > 0 } token
                || _identityOptionsMonitor.CurrentValue.GroupsEndpoint is not { Length: > 0 } groupsEndpoint)
            {
                return [];
            }

            using var client = _httpClientFactory.CreateClient(OAuth2IntrospectionDefaults.BackChannelHttpClientName);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var uri = new Uri(System.IO.Path.Combine(groupsEndpoint,
                $"{groupId}/users"));
            using var response = await client.GetAsync(uri);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<GroupMembersResponse>();

            return result?.Data?.Select(d => d.UserId).OfType<string>() ?? [];
        }

        public async Task<List<string>> GetCachedGroupMembers(string groupId,
        CancellationToken cancellationToken = default)
        {
            if (_identityOptionsMonitor.CurrentValue.GroupsEndpoint is { Length: > 0 }
                && _identityOptionsMonitor.CurrentValue.GroupReaderToken is { Length: > 0 })
            {
                return [];
            }

            return await _appDbContext.IdentityAnnouncementGroups
                    .Where(iag => iag.Groups.Contains(groupId)
                        && iag.LastChangeDateTimeUtc.CompareTo(DateTime.UtcNow.AddDays(-1 * _identityOptionsMonitor.CurrentValue.GroupCacheExpirationInHours)) > 0)
                    .Select(iag => iag.IdentityId)
                    .ToListAsync(cancellationToken);
        }

        public string? GetRegistrationId(ClaimsIdentity identity)
        {
            return identity.FindFirst(UserRegistrationClaims.Id)?.Value;
        }

        private void AddRegistrationToClaims(ClaimsIdentity identity, RegistrationData registrationData)
        {
            if (registrationData.Id is null)
            {
                return;
            }

            identity.AddClaim(new Claim(UserRegistrationClaims.Id, registrationData.Id));
            identity.AddClaim(new Claim(UserRegistrationClaims.Status(registrationData.Id), registrationData.Status.ToString()));

            identity.AddClaim(new Claim(identity.RoleClaimType, IdentityRoles.Attendee));

            if (registrationData.Status == UserRegistrationStatus.CheckedIn)
            {
                identity.AddClaim(new Claim(identity.RoleClaimType, IdentityRoles.AttendeeCheckedIn));
            }
        }
        private async Task<string?> GetRegistrationId(string token)
        {
            using var client = _httpClientFactory.CreateClient(OAuth2IntrospectionDefaults.BackChannelHttpClientName);

            var request = new HttpRequestMessage(HttpMethod.Get,
                new Uri(new Uri($"{_identityOptionsMonitor.CurrentValue.RegSysUrl.TrimEnd('/')}/"),
                    "attsrv/api/rest/v1/attendees"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await client.SendAsync(request);

            // 404 Not found -> no registration for user
            // see: https://github.com/eurofurence/reg-attendee-service/blob/07c1444f70d7afbb9cbd61780f2ecdd801e0c23a/api/openapi-spec/openapi.yaml#L91
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    throw new RegistrationSystemClientException($"Failed to get registration ID from regsys with status code {response.StatusCode}.");
                }
            }

            var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
            return json.RootElement.TryGetStringArray("ids").FirstOrDefault();
        }

        /// <summary>
        /// Retrieve registration status for given ID from registration system.
        /// </summary>
        /// <param name="token">Used to authenticated against the registration system with user's permissions to view own registration.</param>
        /// <param name="id">Registration ID to check status of.</param>
        /// <returns>Status information for registration ID, <c>UserRegistrationStatus.Unknown</c> if provided <c>id</c> is null or throws if request to fetch status for registration ID was unsuccessful.</returns>
        /// <exception cref="RegistrationSystemClientException">
        /// Thrown when an error is encountered while fetching registration status from the regsys backend.
        /// </exception>
        private async Task<RegistrationData> GetRegistrationStatus(string token, string? id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return new RegistrationData(null, UserRegistrationStatus.Unknown);
            }

            using var client = _httpClientFactory.CreateClient(OAuth2IntrospectionDefaults.BackChannelHttpClientName);

            var statusRequest = new HttpRequestMessage(HttpMethod.Get,
                new Uri(new Uri($"{_identityOptionsMonitor.CurrentValue.RegSysUrl.TrimEnd('/')}/"),
                    $"attsrv/api/rest/v1/attendees/{id}/status"));
            statusRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            using var statusResponse = await client.SendAsync(statusRequest);

            if (!statusResponse.IsSuccessStatusCode)
            {
                throw new RegistrationSystemClientException($"Failed to get registration information from regsys with status code {statusResponse.StatusCode}.");
            }

            var statusJson = await JsonDocument.ParseAsync(await statusResponse.Content.ReadAsStreamAsync());
            Enum.TryParse(statusJson.RootElement.TryGetString("status")?.Replace(" ", ""), true,
                out UserRegistrationStatus status);

            return new RegistrationData(id, status);
        }

        private async Task UpdateRegistrationInDatabase(RegistrationData registrationData, ClaimsIdentity identity)
        {
            var identityId = identity.FindFirst("sub")?.Value;
            var nickname = identity.FindFirst("name")?.Value;

            if (string.IsNullOrEmpty(identityId) ||
                string.IsNullOrEmpty(nickname))
            {
                return;
            }

            if (await _appDbContext.Users
                .FirstOrDefaultAsync(x => x.IdentityId == identityId) is { } user)
            {
                if (user.RegSysId != registrationData?.Id ||
                user.RegistrationStatus != registrationData?.Status)
                {
                    user.RegSysId = registrationData?.Id;
                    user.RegistrationStatus = registrationData?.Status ?? UserRegistrationStatus.Unknown;
                    user.Touch();
                }
            }
            else
            {
                _appDbContext.Users.Add(new UserRecord
                {
                    RegSysId = registrationData?.Id,
                    IdentityId = identityId,
                    Nickname = nickname,
                    RegistrationStatus = registrationData?.Status ?? UserRegistrationStatus.Unknown
                });
            }

            await _appDbContext.SaveChangesAsync();
        }

        private sealed class RegistrationData(string? id, UserRegistrationStatus status = UserRegistrationStatus.Unknown)
        {
            public string? Id { get; init; } = id;
            public UserRegistrationStatus Status { get; init; } = status;
        }

        private sealed class CachedClaim(string type, string value)
        {
            public string Type { get; set; } = type;

            public string Value { get; set; } = value;
        }

        private sealed class GroupMembersResponse : ProtocolResponse
        {
            [JsonPropertyName("data")] public GroupMemberResponseData[]? Data { get; set; }
        }

        private sealed class GroupMemberResponseData
        {
            [JsonPropertyName("group_id")] public string? GroupId { get; set; }

            [JsonPropertyName("user_id")] public string? UserId { get; set; }

            [JsonPropertyName("level")] public string? Level { get; set; }
        }

        [Serializable]
        public class RegistrationSystemClientException : Exception
        {
            public RegistrationSystemClientException() { }
            public RegistrationSystemClientException(string message) : base(message) { }
            public RegistrationSystemClientException(string message, Exception inner) : base(message, inner) { }
        }
    }
}