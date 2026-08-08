#nullable enable
using System.Collections.Generic;

namespace Eurofurence.App.Server.Services.Abstractions.AppConfig
{
    public class AppConfigOptions
    {
        /// <summary>
        /// Currently latest app version published; used to ask users to update if a new version
        /// is available.
        /// </summary>
        public string LatestRelease { get; init; } = "0.0.0";

        /// <summary>
        /// URL the Map button in the app will open.
        /// Maps button will disappear if <c>null</c>.
        /// </summary>
        public string? MapsUrl { get; init; }

        /// <summary>
        /// URL the Catch-Em-All (CMA) button in the app will open.
        /// Hides button if <c>null</c>.
        /// </summary>
        public string? CmaUrl { get; init; }

        /// <summary>
        /// URL the Critter Management System (CMS) button in the app will open.
        /// Hides button if <c>null</c>.
        /// </summary>
        public string? CritterUrl { get; init; }

        /// <summary>
        /// Overrides the default SSID for the public WiFi if needed.
        /// </summary>
        public string? PublicWifiSsid { get; init; }

        /// <summary>
        /// URL the Weather button in the app will open.
        /// Hides button if <c>null</c>.
        /// </summary>
        public string? WeatherUrl { get; init; }

        /// <summary>
        /// Explicitly set feature flag values. All feature flags must have a working default
        /// in the app and not result in errors if not explicitly set in the backend.
        /// </summary>
        public Dictionary<string, FeatureFlag> FeatureFlags { get; init; } = new();
    }
}
