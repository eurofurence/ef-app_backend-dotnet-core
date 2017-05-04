using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace Eurofurence.App.Server.Services.Abstractions
{
    public interface ITokenFactory
    {
        string CreateTokenFromClaims(IEnumerable<Claim> claims, DateTime expiration);
    }
}