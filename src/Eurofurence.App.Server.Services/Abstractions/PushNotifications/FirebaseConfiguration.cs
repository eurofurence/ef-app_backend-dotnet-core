using Microsoft.Extensions.Configuration;

namespace Eurofurence.App.Server.Services.Abstractions.PushNotifications
{
    public class FirebaseConfiguration
    {
        public bool IsConfigured => !string.IsNullOrEmpty(AuthorizationKey);
        public string AuthorizationKey { get; set; }
        public string ExpoExperienceId { get; set; }
        public string ExpoScopeKey { get; set; }

        public static FirebaseConfiguration FromConfiguration(IConfiguration configuration)
             => new FirebaseConfiguration
             {
                 AuthorizationKey = configuration["firebase:authorizationKey"],
                 ExpoExperienceId = configuration["firebase:expo:experienceId"],
                 ExpoScopeKey = configuration["firebase:expo:scopeKey"]
             };
    }
}