using Microsoft.Extensions.Configuration;

namespace Eurofurence.App.Server.Services.Abstractions.Security
{
    public class TokenFactorySettings
    {
        public string SecretKey { get; set; }
        public string Audience { get; set; }
        public string Issuer { get; set; }

        public static TokenFactorySettings FromConfiguration(IConfiguration configuration)
            => new TokenFactorySettings
            {
                SecretKey = configuration["oAuth:secretKey"],
                Audience = configuration["oAuth:audience"],
                Issuer = configuration["oAuth:issuer"]
            };
    }
}