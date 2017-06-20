namespace Eurofurence.App.Server.Services.Abstractions.Security
{
    public class TokenFactorySettings
    {
        public string SecretKey { get; set; }
        public string Audience { get; set; }
        public string Issuer { get; set; }
    }
}