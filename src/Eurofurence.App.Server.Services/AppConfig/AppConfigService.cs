using Eurofurence.App.Domain.Model.AppConfig;
using Eurofurence.App.Server.Services.Abstractions.AppConfig;
using Microsoft.Extensions.Options;

namespace Eurofurence.App.Server.Services.AppConfig
{
    public class AppConfigService : IAppConfigService
    {
        private readonly IOptionsMonitor<AppConfigOptions> _appConfigOptions;
        private AppConfigData _appConfig;
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
                { "LatestRelease", appConfigOptions.LatestRelease },
                { "MapsUrl", appConfigOptions.MapsUrl },
                { "CmaUrl", appConfigOptions.CmaUrl }
            };

            foreach (var featureFlag in appConfigOptions.FeatureFlags)
            {
                var featureFlagName = FeatureFlag.GetName(featureFlag);
                appConfig.Add(featureFlagName, featureFlag.Value.Value);
            }
            _appConfig = appConfig;
        }
    }
}
