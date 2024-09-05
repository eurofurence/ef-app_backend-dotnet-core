using Microsoft.Extensions.Configuration;

namespace Eurofurence.App.Server.Services.Abstractions.PushNotifications
{
    public class FirebaseConfiguration
    {
        public bool IsConfigured => !string.IsNullOrWhiteSpace(GoogleServiceCredentialKeyFile);
        public string GoogleServiceCredentialKeyFile { get; set; }
        public string ExpoExperienceId { get; set; }
        public string ExpoScopeKey { get; set; }

        public static FirebaseConfiguration FromConfiguration(IConfiguration configuration)
             => new FirebaseConfiguration
             {
                 GoogleServiceCredentialKeyFile = configuration["push:firebase:googleServiceCredentialKeyFile"],
                 ExpoExperienceId = configuration["push:firebase:expo:experienceId"],
                 ExpoScopeKey = configuration["push:firebase:expo:scopeKey"]
             };
    }
}