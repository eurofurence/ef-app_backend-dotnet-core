using Microsoft.Extensions.Configuration;
using System;

namespace Eurofurence.App.Server.Services.Abstractions.Security
{
    public class AuthenticationSettings
    {
        public TimeSpan DefaultTokenLifeTime { get; set; }

        public static AuthenticationSettings FromConfiguration(IConfiguration configuration)
            => new AuthenticationSettings
            {
                DefaultTokenLifeTime = TimeSpan.FromDays(30)
            };
    }
}