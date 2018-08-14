using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Security;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace Eurofurence.App.Server.Services.Security
{
    public class AuthenticationHandler : IAuthenticationHandler
    {
        private readonly ILogger _logger;
        private readonly ConventionSettings _conventionSettings;
        private readonly AuthenticationSettings _authenticationSettings;
        private readonly IEntityRepository<RegSysIdentityRecord> _regSysIdentityRepository;
        private readonly IEntityRepository<RegSysAccessTokenRecord> _regSysAccessTokenRepository;
        private readonly ITokenFactory _tokenFactory;
        private readonly static Random _random = new Random();

        private readonly IAuthenticationProvider[] _authenticationProviders;

        public AuthenticationHandler(
            ILoggerFactory loggerFactory,
            ConventionSettings conventionSettings,
            AuthenticationSettings authenticationSettings,
            IEntityRepository<RegSysAlternativePinRecord> regSysAlternativePinRepository,
            IEntityRepository<RegSysIdentityRecord> regSysIdentityRepository,
            IEntityRepository<RegSysAccessTokenRecord> regSysAccessTokenRepository,
            ITokenFactory tokenFactory
        )
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _conventionSettings = conventionSettings;
            _authenticationSettings = authenticationSettings;
            _regSysIdentityRepository = regSysIdentityRepository;
            _regSysAccessTokenRepository = regSysAccessTokenRepository;
            _tokenFactory = tokenFactory;

            _authenticationProviders =
                _conventionSettings.IsRegSysAuthenticationEnabled ?
                    new IAuthenticationProvider[]
                        {
                            new RegSysAlternativePinAuthenticationProvider(regSysAlternativePinRepository),
                            new RegSysCredentialsAuthenticationProvider()
                        }
                    :
                    new IAuthenticationProvider[]
                        {
                            new RegSysAlternativePinAuthenticationProvider(regSysAlternativePinRepository),
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

            var uid = $"RegSys:{_conventionSettings.ConventionNumber}:{authenticationResult.RegNo}";

            var identityRecord = await _regSysIdentityRepository.FindOneAsync(a => a.Uid == uid);
            if (identityRecord == null)
            {
                identityRecord = new RegSysIdentityRecord
                {
                    Id = Guid.NewGuid(),
                    Uid = uid,
                    Roles = new List<string>() { "Attendee" }
                };
                await _regSysIdentityRepository.InsertOneAsync(identityRecord);
            }

            if (!String.IsNullOrWhiteSpace(request.AccessToken))
            {
                var accessToken = await _regSysAccessTokenRepository.FindOneAsync(a => a.Token == request.AccessToken);
                if (accessToken != null && !accessToken.ClaimedAtDateTimeUtc.HasValue)
                {
                    identityRecord.Roles = identityRecord.Roles
                        .Concat(accessToken.GrantRoles)
                        .Distinct()
                        .ToArray();

                    accessToken.ClaimedByUid = identityRecord.Uid;
                    accessToken.ClaimedAtDateTimeUtc = DateTime.UtcNow;

                    await _regSysAccessTokenRepository.ReplaceOneAsync(accessToken);
                }
            }

            identityRecord.Username = authenticationResult.Username;
            await _regSysIdentityRepository.ReplaceOneAsync(identityRecord);


            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, uid),
                new Claim(ClaimTypes.GivenName, authenticationResult.Username),
                new Claim(ClaimTypes.PrimarySid, authenticationResult.RegNo.ToString()),
                new Claim(ClaimTypes.GroupSid, _conventionSettings.ConventionNumber.ToString()),
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

            await _regSysAccessTokenRepository.InsertOneAsync(accessToken);
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