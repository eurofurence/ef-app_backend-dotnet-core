using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Eurofurence.App.Server.Services.Abstractions.Security;

namespace Eurofurence.App.Server.Services.Security
{
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
            // Temporary little hack...
            //var isValid = await _registrationSystemAuthenticationBridge.VerifyCredentialSetAsync(
            //  request.RegNo, request.Username, request.Password);

            await Task.Delay(1);
            var isValid = request.Password == request.RegNo.ToString();

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
