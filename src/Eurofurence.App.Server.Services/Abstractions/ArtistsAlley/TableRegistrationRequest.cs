using System;

namespace Eurofurence.App.Server.Services.Abstractions.ArtistsAlley
{
    public class TableRegistrationRequest
    {
        public string DisplayName { get; set; }

        public string WebsiteUrl { get; set; }

        public string ShortDescription { get; set; }

        public Guid ImageId { get; set; }

        public string Location { get; set; }

        public string TelegramHandle { get; set; }
    }
}
