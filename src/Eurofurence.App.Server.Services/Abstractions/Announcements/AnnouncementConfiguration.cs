using Microsoft.Extensions.Configuration;

namespace Eurofurence.App.Server.Services.Abstractions.Announcements
{
    public class AnnouncementConfiguration
    {
        public string Url { get; set; }

        public static AnnouncementConfiguration FromConfiguration(IConfiguration configuration)
            => new()
            {
                Url = configuration["announcements:url"]
            };
    }
}
