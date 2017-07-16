using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Eurofurence.App.Domain.Model.Abstractions;
using Eurofurence.App.Domain.Model.Security;
using Eurofurence.App.Server.Services.Abstractions;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Microsoft.Extensions.Logging;

namespace Eurofurence.App.Server.Services.Security
{
    public class AuthenticationHandler : IAuthenticationHandler
    {
        private readonly ILogger _logger;
        private readonly ConventionSettings _conventionSettings;
        private readonly AuthenticationSettings _authenticationSettings;
        private readonly ITokenFactory _tokenFactory;

        private readonly RegSysCredentialsAuthenticationProvider _regSysCredentialsAuthenticationProvider;
        private readonly RegSysAlternativePinAuthenticationProvider _regSysAlternativePinAuthenticationProvider;

        private readonly IAuthenticationProvider[] _authenticationProviders;

        public AuthenticationHandler(
            ILoggerFactory loggerFactory,
            ConventionSettings conventionSettings,
            AuthenticationSettings authenticationSettings,
            IEntityRepository<RegSysAlternativePinRecord> regSysAlternativePinRepository,
            ITokenFactory tokenFactory
        )
        {
            _logger = loggerFactory.CreateLogger(GetType());
            _conventionSettings = conventionSettings;
            _authenticationSettings = authenticationSettings;
            _tokenFactory = tokenFactory;

            _authenticationProviders = new IAuthenticationProvider[]
            {
                new RegSysAlternativePinAuthenticationProvider(regSysAlternativePinRepository),
                new RegSysCredentialsAuthenticationProvider()
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
                _logger.LogWarning("Authentication failed for {Username} {RegNo}", request.Username, request.RegNo);
                return null;
            }

            var uid = $"RegSys:{_conventionSettings.ConventionNumber}:{authenticationResult.RegNo}";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, uid),
                new Claim(ClaimTypes.GivenName, authenticationResult.Username.ToLower()),
                new Claim(ClaimTypes.PrimarySid, authenticationResult.RegNo.ToString()),
                new Claim(ClaimTypes.GroupSid, _conventionSettings.ConventionNumber.ToString()),
                new Claim(ClaimTypes.Role, "Attendee"),
                new Claim(ClaimTypes.System, "RegSys")
            };

            var expiration = DateTime.UtcNow.Add(_authenticationSettings.DefaultTokenLifeTime);
            var token = _tokenFactory.CreateTokenFromClaims(claims, expiration);

            var response = new AuthenticationResponse
            {
                Uid = uid,
                Token = token,
                TokenValidUntil = expiration,
                Username = $"{authenticationResult.Username.ToLower()} ({authenticationResult.RegNo})"
            };

            _logger.LogInformation("Authentication successful for {Username} {RegNo} via {Source}",
                authenticationResult.Username, authenticationResult.RegNo, authenticationResult.Source);

            return response;
        }
    }
}