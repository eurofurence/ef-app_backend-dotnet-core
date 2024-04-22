using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Security;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Microsoft.Extensions.Logging;
using System.Linq;
using Eurofurence.App.Infrastructure.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Eurofurence.App.Server.Services.Security
{
    public class AuthenticationHandler : IAuthenticationHandler
    {
        private readonly AppDbContext _appDbContext;
        private readonly ILogger _logger;
        private readonly ConventionSettings _conventionSettings;
        private readonly AuthenticationSettings _authenticationSettings;
        private readonly ITokenFactory _tokenFactory;
        private static readonly Random _random = new Random();

        private readonly IAuthenticationProvider[] _authenticationProviders;

        public AuthenticationHandler(
            AppDbContext appDbContext,
            ILoggerFactory loggerFactory,
            ConventionSettings conventionSettings,
            AuthenticationSettings authenticationSettings,
            ITokenFactory tokenFactory
        )
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _conventionSettings = conventionSettings;
            _authenticationSettings = authenticationSettings;
            _tokenFactory = tokenFactory;

            _authenticationProviders =
                _conventionSettings.IsRegSysAuthenticationEnabled ?
                    new IAuthenticationProvider[]
                        {
                            new RegSysAlternativePinAuthenticationProvider(appDbContext),
                            new RegSysCredentialsAuthenticationProvider()
                        }
                    :
                    new IAuthenticationProvider[]
                        {
                            new RegSysAlternativePinAuthenticationProvider(appDbContext),
                        };
        }


        public async Task<AuthenticationResponse> AuthorizeViaRegSys(RegSysAuthenticationRequest request)
        {
            AuthenticationResult authenticationResult = null;
            foreach (var provider in _authenticationProviders)
            {
                var providerResult = await provider.ValidateRegSysAuthenticationRequestAsync(request);
                if (providerResult.IsAuthenticated)
                {
                    authenticationResult = providerResult;
                    break;
                }
            }

            if (authenticationResult == null)
            {
                _logger.LogWarning(LogEvents.Audit, "Authentication failed for {Username} {RegNo}", request.Username, request.RegNo);
                return null;
            }

            var uid = $"RegSys:{_conventionSettings.ConventionIdentifier}:{authenticationResult.RegNo}";

            var identityRecord = await _appDbContext.RegSysIdentities.FirstOrDefaultAsync(a => a.Uid == uid);
            if (identityRecord == null)
            {
                identityRecord = new RegSysIdentityRecord
                {
                    Id = Guid.NewGuid(),
                    Uid = uid,
                    Roles = new List<string>() { "Attendee" }
                };
                _appDbContext.RegSysIdentities.Add(identityRecord);

            }

            if (!String.IsNullOrWhiteSpace(request.AccessToken))
            {
                var accessToken = await _appDbContext.RegSysAccessTokens.FirstOrDefaultAsync(a => a.Token == request.AccessToken);
                if (accessToken != null && !accessToken.ClaimedAtDateTimeUtc.HasValue)
                {
                    identityRecord.Roles = identityRecord.Roles
                        .Concat(accessToken.GrantRoles)
                        .Distinct()
                        .ToArray();

                    accessToken.ClaimedByUid = identityRecord.Uid;
                    accessToken.ClaimedAtDateTimeUtc = DateTime.UtcNow;

                    _appDbContext.RegSysAccessTokens.Update(accessToken);
                }
            }

            identityRecord.Username = authenticationResult.Username;
            _appDbContext.RegSysIdentities.Update(identityRecord);
            

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, uid),
                new Claim(ClaimTypes.GivenName, authenticationResult.Username),
                new Claim(ClaimTypes.PrimarySid, authenticationResult.RegNo.ToString()),
                new Claim(ClaimTypes.GroupSid, _conventionSettings.ConventionIdentifier.ToString()),
                new Claim(ClaimTypes.System, "RegSys")
            };

            claims.AddRange(identityRecord.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var expiration = DateTime.UtcNow.Add(_authenticationSettings.DefaultTokenLifeTime);
            var token = _tokenFactory.CreateTokenFromClaims(claims, expiration);

            var response = new AuthenticationResponse
            {
                Uid = uid,
                Token = token,
                TokenValidUntil = expiration,
                Username = $"{authenticationResult.Username} ({authenticationResult.RegNo})"
            };

            _logger.LogInformation(LogEvents.Audit, "Authentication successful for {Username} {RegNo} ({Uid}) via {Source}",
                authenticationResult.Username, authenticationResult.RegNo, response.Uid, authenticationResult.Source);

            await _appDbContext.SaveChangesAsync();

            return response;
        }

        public async Task<string> CreateRegSysAccessTokenAsync(string[] rolesToGrant)
        {
            string chars = "abcdefghijklmnopqrstuvwxyz0123456789";

            var accessToken = new RegSysAccessTokenRecord()
            {
                Id = Guid.NewGuid(),
                GrantRoles = rolesToGrant,
                Token = new string(Enumerable.Repeat(chars, 10).Select(s => s[_random.Next(s.Length)]).ToArray())
            };

            _appDbContext.RegSysAccessTokens.Add(accessToken);
            await _appDbContext.SaveChangesAsync();
            return accessToken.Token;
        }

        public AuthenticationResponse AuthorizeViaPrincipal(IApiPrincipal principal)
        {
            var result = new AuthenticationResponse()
            {
                Uid = principal.Uid,
                Username = principal.DisplayName,
                TokenValidUntil = principal.AuthenticationValidUntilUtc ?? DateTime.MinValue
            };

            return result;
        }
    }
}