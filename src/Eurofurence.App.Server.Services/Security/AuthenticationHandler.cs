using Eurofurence.App.Server.Services.Abstractions;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Eurofurence.App.Server.Services.Security
{
    public class RegSysAuthenticationRequest
    {
        public int RegNo { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class AuthenticationResponse
    {
        public string Uid { get; set; }
        public string Username { get; set; }
        public string Token { get; set; }
        public DateTime TokenValidUntil { get; set; }
    }

    public class AuthenticationHandler : IAuthenticationHandler
    {
        readonly IRegSysAuthenticationBridge _registrationSystemAuthenticationBridge;
        readonly ITokenFactory _tokenFactory;
        readonly AuthenticationSettings _authenticationSettings;

        public AuthenticationHandler(
            AuthenticationSettings authenticationSettings,
            IRegSysAuthenticationBridge registrationSystemAuthenticationBridge,
            ITokenFactory tokenFactory         
            )
        {
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
                return null;
            }

            var uid = $"RegSys:{_authenticationSettings.ConventionNumber}:{request.RegNo}";

            var claims = new List<Claim>()
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

            var response = new AuthenticationResponse()
            {
                Uid = uid,
                Token = token,
                TokenValidUntil = expiration,
                Username = $"{request.Username.ToLower()} ({request.RegNo})"
            };

            return response;
        }

    }
}
