using Eurofurence.App.Server.Services.Abstractions;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Eurofurence.App.Server.Services.Abstractions.Security;

namespace Eurofurence.App.Server.Services.Security
{
    public class TokenFactory : ITokenFactory
    {
        readonly TokenFactorySettings _tokenFactorySettings;

        public TokenFactory(TokenFactorySettings tokenFactorySettings)
        {
            _tokenFactorySettings = tokenFactorySettings;
        }

        public string CreateTokenFromClaims(IEnumerable<Claim> claims, DateTime expiration)
        {
            var identity = new ClaimsIdentity(claims);
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_tokenFactorySettings.SecretKey));

            var descriptor = new SecurityTokenDescriptor()
            {
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
                Audience = _tokenFactorySettings.Audience,
                Issuer = _tokenFactorySettings.Issuer,
                Subject = identity,

                NotBefore = DateTime.UtcNow.AddMinutes(-5),
                IssuedAt = DateTime.UtcNow.AddMinutes(-5),
                Expires = expiration
            };

            var handler = new JwtSecurityTokenHandler();
            var securityToken = handler.CreateToken(descriptor);

            var token = handler.WriteToken(securityToken);

            return token;
        }
    }
}
