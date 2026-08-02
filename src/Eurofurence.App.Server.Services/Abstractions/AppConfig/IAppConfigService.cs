using Eurofurence.App.Domain.Model.AppConfig;

namespace Eurofurence.App.Server.Services.Abstractions.AppConfig
{
    public interface IAppConfigService
    {
        AppConfigData Get();
    }
}
