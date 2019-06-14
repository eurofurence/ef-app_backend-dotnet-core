using Microsoft.Extensions.Configuration;

namespace Eurofurence.App.Server.Services.Abstractions.ArtistsAlley
{
    public class ArtistAlleyConfiguration
    {
        public string TelegramAdminGroupChatId { get; set; }
        public string TelegramAnnouncementChannelId { get; set; }
        public string TwitterConsumerKey { get; set; }
        public string TwitterConsumerSecret { get; set; }
        public string TwitterAccessToken { get; set; }
        public string TwitterAccessTokenSecret { get; set; }

        public static ArtistAlleyConfiguration FromConfiguration(IConfiguration configuration)
            => new ArtistAlleyConfiguration()
            {
                TelegramAdminGroupChatId = configuration["artistAlley:telegram:adminGroupChatId"],
                TelegramAnnouncementChannelId = configuration["artistAlley:telegram:announcementChannelId"],
                TwitterConsumerKey = configuration["artistAlley:twitter:consumerKey"],
                TwitterConsumerSecret = configuration["artistAlley:twitter:consumerSecret"],
                TwitterAccessToken = configuration["artistAlley:twitter:accessToken"],
                TwitterAccessTokenSecret = configuration["artistAlley:twitter:accessTokenSecret"]
            };
    }
}
