using Microsoft.Extensions.Configuration;

namespace Eurofurence.App.Server.Services.Abstractions.Dealers
{
    public class DealerConfiguration
    {
        public string Url { get; set; }
        public string User { get; set; }
        public string Password { get; set; }

        public static DealerConfiguration FromConfiguration(IConfiguration configuration)
            => new DealerConfiguration
            {
                Url = configuration["dealers:url"],
                User = configuration["dealers:user"],
                Password = configuration["dealers:password"],
            };
    }
}
