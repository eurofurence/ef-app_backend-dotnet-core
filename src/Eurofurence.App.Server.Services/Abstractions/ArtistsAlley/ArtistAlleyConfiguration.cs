using Microsoft.Extensions.Configuration;

namespace Eurofurence.App.Server.Services.Abstractions.ArtistsAlley
{
    public class ArtistAlleyConfiguration
    {
        public string TelegramAdminGroupChatId { get; set; }
        public string TelegramAnnouncementChannelId { get; set; }

        public bool RegistrationEnabled { get; set; }
        
        public static ArtistAlleyConfiguration FromConfiguration(IConfiguration configuration)
            => new ArtistAlleyConfiguration()
            {
                TelegramAdminGroupChatId = configuration["artistAlley:telegram:adminGroupChatId"],
                TelegramAnnouncementChannelId = configuration["artistAlley:telegram:announcementChannelId"],
                RegistrationEnabled = bool.Parse(configuration["artistAlley:registrationEnabled"])
            };
    }
}
