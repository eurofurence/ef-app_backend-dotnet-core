using Microsoft.Extensions.Configuration;

namespace Eurofurence.App.Server.Services.Abstractions.PushNotifications
{
    public class FirebaseConfiguration
    {
        public string AuthorizationKey { get; set; }

        public static FirebaseConfiguration FromConfiguration(IConfiguration configuration)
             => new FirebaseConfiguration
             {
                 AuthorizationKey = configuration["firebase:authorizationKey"]
             };
    }
}