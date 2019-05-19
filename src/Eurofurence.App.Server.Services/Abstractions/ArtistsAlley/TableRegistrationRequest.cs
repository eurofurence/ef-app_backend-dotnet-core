namespace Eurofurence.App.Server.Services.Abstractions.ArtistsAlley
{
    public class TableRegistrationRequest
    {
        public string DisplayName { get; set; }

        public string WebsiteUrl { get; set; }

        public string ShortDescription { get; set; }

        public string ImageContent { get; set; }

        public string Location { get; set; }

        public string TelegramHandle { get; set; }
    }
}
