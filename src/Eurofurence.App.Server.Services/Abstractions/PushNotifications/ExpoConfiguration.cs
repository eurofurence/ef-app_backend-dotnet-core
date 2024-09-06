using Microsoft.Extensions.Configuration;

namespace Eurofurence.App.Server.Services.Abstractions.PushNotifications
{
    public class ExpoConfiguration
    {
        public bool IsConfigured => !string.IsNullOrWhiteSpace(ExperienceId) && !string.IsNullOrWhiteSpace(ScopeKey);
        public string ExperienceId { get; set; }
        public string ScopeKey { get; set; }

        public static ExpoConfiguration FromConfiguration(IConfiguration configuration)
             => new ExpoConfiguration
             {
                 ExperienceId = configuration["push:expo:experienceId"],
                 ScopeKey = configuration["push:expo:scopeKey"]
             };
    }
}