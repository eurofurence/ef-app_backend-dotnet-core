#nullable enable
using Eurofurence.App.Domain.Model.AppConfig;
using Eurofurence.App.Server.Services.Abstractions.AppConfig;
using Microsoft.Extensions.Options;

namespace Eurofurence.App.Server.Services.AppConfig
{
    public class AppConfigService : IAppConfigService
    {
        private readonly IOptionsMonitor<AppConfigOptions> _appConfigOptions;
        private AppConfigData _appConfig = new();
        public AppConfigService(
            IOptionsMonitor<AppConfigOptions> appConfigOptions
            )
        {
            _appConfigOptions = appConfigOptions;
            _appConfigOptions.OnChange(UpdateAppConfig);
            UpdateAppConfig(_appConfigOptions.CurrentValue);
        }
        public AppConfigData Get()
        {
            return _appConfig;
        }

        private void UpdateAppConfig(AppConfigOptions appConfigOptions)
        {
            AppConfigData appConfig = new()
            {
                { "LatestRelease", appConfigOptions.LatestRelease }
            };

            if (appConfigOptions.MapsUrl is string mapsUrl)
            {
                appConfig.Add("MapsUrl", mapsUrl);
            }

            if (appConfigOptions.CmaUrl is string cmaUrl)
            {
                appConfig.Add("CmaUrl", cmaUrl);
            }

            if (appConfigOptions.CritterUrl is string critterUrl)
            {
                appConfig.Add("CritterUrl", critterUrl);
            }

            if (appConfigOptions.PublicWifiSsid is string publicWifiSsid)
            {
                appConfig.Add("PublicWifiSsid", publicWifiSsid);
            }

            if (appConfigOptions.WeatherUrl is string weatherUrl)
            {
                appConfig.Add("WeatherUrl", weatherUrl);
            }


            foreach (var featureFlag in appConfigOptions.FeatureFlags)
            {
                if (featureFlag.Value.Value is string featureFlagValue)
                {
                    var featureFlagName = FeatureFlag.GetName(featureFlag);
                    appConfig.Add(featureFlagName, featureFlagValue);
                }
            }
            _appConfig = appConfig;
        }
    }
}
