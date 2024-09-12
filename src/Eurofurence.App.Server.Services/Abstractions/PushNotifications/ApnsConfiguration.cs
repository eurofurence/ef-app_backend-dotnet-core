using dotAPNS;
using Microsoft.Extensions.Configuration;

namespace Eurofurence.App.Server.Services.Abstractions.PushNotifications
{
    public class ApnsConfiguration
    {
        public bool IsConfigured => !string.IsNullOrWhiteSpace(BundleId) && !string.IsNullOrWhiteSpace(CertContent) && !string.IsNullOrWhiteSpace(KeyId) && !string.IsNullOrWhiteSpace(TeamId);
        public string BundleId { get; private set; }
        public string CertFilePath { get; private set; }
        public string CertContent { get; private set; }
        public string KeyId { get; private set; }
        public string TeamId { get; private set; }
        public bool UseDevelopmentServer { get; private set; }
        public ApnsJwtOptions ApnsJwtOptions { get; private set; }

        public static ApnsConfiguration FromConfiguration(IConfiguration configuration)
        {

            var apnsConfiguration = new ApnsConfiguration
            {
                BundleId = configuration["push:apns:bundleId"],
                CertContent = configuration["push:apns:certContent"],
                KeyId = configuration["push:apns:keyId"],
                TeamId = configuration["push:apns:teamId"],
                UseDevelopmentServer = bool.TryParse(configuration["push:apns:useDevelopmentServer"], out bool shouldUseDevelopmentServer) ? shouldUseDevelopmentServer : true,
            };

            if (apnsConfiguration.IsConfigured)
            {
                apnsConfiguration.ApnsJwtOptions = new ApnsJwtOptions()
                {
                    BundleId = apnsConfiguration.BundleId,
                    CertContent = apnsConfiguration.CertContent,
                    KeyId = apnsConfiguration.KeyId,
                    TeamId = apnsConfiguration.TeamId,
                };
            }

            return apnsConfiguration;
        }
    }
}