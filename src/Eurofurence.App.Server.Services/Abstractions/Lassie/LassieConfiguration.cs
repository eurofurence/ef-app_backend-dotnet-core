using Microsoft.Extensions.Configuration;

namespace Eurofurence.App.Server.Services.Abstractions.Lassie
{
    public class LassieConfiguration
    {
        public bool IsConfigured => !string.IsNullOrEmpty(ApiKey);
        public string BaseApiUrl { get; set; }
        public string ApiKey { get; set; }

        public static LassieConfiguration FromConfiguration(IConfiguration configuration)
             => new LassieConfiguration
             {
                 BaseApiUrl = configuration["lassie:baseApiUrl"],
                 ApiKey = configuration["lassie:apiKey"]
             };
    }
}
