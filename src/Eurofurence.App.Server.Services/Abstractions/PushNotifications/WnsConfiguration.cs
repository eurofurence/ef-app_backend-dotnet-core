using Microsoft.Extensions.Configuration;

namespace Eurofurence.App.Server.Services.Abstractions.PushNotifications
{
    public class WnsConfiguration
    {
        public bool IsConfigured => !string.IsNullOrEmpty(ClientId) 
            && !string.IsNullOrEmpty(ClientSecret)
            && !string.IsNullOrEmpty(TargetTopic);

        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string TargetTopic { get; set; }

        public static WnsConfiguration FromConfiguration(IConfiguration configuration)
            => new WnsConfiguration
            {
                ClientId = configuration["wns:clientId"],
                ClientSecret = configuration["wns:clientSecret"],
                TargetTopic = configuration["wns:targetTopic"]
            };
    }
}