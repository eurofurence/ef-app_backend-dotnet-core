using Microsoft.Extensions.Configuration;

namespace Eurofurence.App.Server.Services.Abstractions.PushNotifications
{
    public class ApnsConfiguration
    {
        public bool IsConfigured => !string.IsNullOrWhiteSpace(BundleId) && !string.IsNullOrWhiteSpace(CertFilePath) && !string.IsNullOrWhiteSpace(KeyId) && !string.IsNullOrWhiteSpace(TeamId);
        public string BundleId { get; set; }
        public string CertFilePath { get; set; }
        public string KeyId { get; set; }
        public string TeamId { get; set; }

        public static ApnsConfiguration FromConfiguration(IConfiguration configuration)
             => new ApnsConfiguration
             {
                BundleId = configuration["push:apns:bundleId"],
                CertFilePath = configuration["push:apns:certFilePath"],
                KeyId = configuration["push:apns:keyId"],
                TeamId = configuration["push:apns:teamId"],
             };
    }
}