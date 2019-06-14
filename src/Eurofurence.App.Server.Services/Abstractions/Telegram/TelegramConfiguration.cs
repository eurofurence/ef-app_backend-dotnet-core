using Microsoft.Extensions.Configuration;

namespace Eurofurence.App.Server.Services.Abstractions.Telegram
{
    public class TelegramConfiguration
    {
        public string AccessToken { get; set; }
        public string Proxy { get; set; }

        public static TelegramConfiguration FromConfiguration(IConfiguration configuration)
            => new TelegramConfiguration
            {
                AccessToken = configuration["telegram:accessToken"],
                Proxy = configuration["telegram:proxy"]
            };
    }
}