using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;

namespace Eurofurence.App.Server.Services.Abstractions.PushNotifications
{
    public class FirebaseConfiguration
    {
        public bool IsConfigured => !string.IsNullOrWhiteSpace(GoogleServiceCredentialKeyFile) && GoogleCredential != null;
        public string GoogleServiceCredentialKeyFile { get; private set; }
        public GoogleCredential GoogleCredential { get; private set; }

        public static FirebaseConfiguration FromConfiguration(IConfiguration configuration)
        {
            var firebaseConfiguration = new FirebaseConfiguration
            {
                GoogleServiceCredentialKeyFile = configuration["push:firebase:googleServiceCredentialKeyFile"]
            };

            if (!string.IsNullOrWhiteSpace(firebaseConfiguration.GoogleServiceCredentialKeyFile)) {
                firebaseConfiguration.GoogleCredential = GoogleCredential.FromFile(firebaseConfiguration.GoogleServiceCredentialKeyFile);
                FirebaseApp.Create(new AppOptions { Credential = firebaseConfiguration.GoogleCredential });
            }

            return firebaseConfiguration;
        }
    }
}