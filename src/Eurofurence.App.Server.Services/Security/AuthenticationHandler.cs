using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Eurofurence.App.Server.Services.Abstractions.Security;
using Microsoft.Extensions.Logging;

namespace Eurofurence.App.Server.Services.Security
{
    public class AuthenticationHandler : IAuthenticationHandler
    {
        private readonly ILogger _logger;
        private readonly AuthenticationSettings _authenticationSettings;
        private readonly IRegSysAuthenticationBridge _registrationSystemAuthenticationBridge;
        private readonly ITokenFactory _tokenFactory;

        public AuthenticationHandler(
            ILoggerFactory loggerFactory,
            AuthenticationSettings authenticationSettings,
            IRegSysAuthenticationBridge registrationSystemAuthenticationBridge,
            ITokenFactory tokenFactory
        )
        {
            _logger = loggerFactory.CreateLogger(GetType().Name);
            _authenticationSettings = authenticationSettings;
            _registrationSystemAuthenticationBridge = registrationSystemAuthenticationBridge;
            _tokenFactory = tokenFactory;
        }

        public async Task<AuthenticationResponse> AuthorizeViaRegSys(RegSysAuthenticationRequest request)
        {
            var isValid = await _registrationSystemAuthenticationBridge.VerifyCredentialSetAsync(
              request.RegNo, request.Username, request.Password);

            if (!isValid)
            {
                _logger.LogWarning("AuthorizeViaRegSys failed for {Username} {RegNo}", request.Username, request.RegNo);
                return null;
            }

            var uid = $"RegSys:{_authenticationSettings.ConventionNumber}:{request.RegNo}";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, uid),
                new Claim(ClaimTypes.GivenName, request.Username.ToLower()),
                new Claim(ClaimTypes.PrimarySid, request.RegNo.ToString()),
                new Claim(ClaimTypes.GroupSid, _authenticationSettings.ConventionNumber.ToString()),
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
                Username = $"{request.Username.ToLower()} ({request.RegNo})"
            };

            _logger.LogInformation("AuthorizeViaRegSys successful for {Username} {RegNo}", request.Username, request.RegNo);

            return response;
        }
    }
}