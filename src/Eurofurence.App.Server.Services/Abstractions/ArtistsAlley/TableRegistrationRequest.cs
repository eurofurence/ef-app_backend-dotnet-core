using System;
using System.Collections.Generic;
using System.Text;

namespace Eurofurence.App.Server.Services.Abstractions.ArtistsAlley
{
    public class TableRegistrationRequest
    {
        public string DisplayName { get; set; }

        public string Merchandise { get; set; }

        public string ShortDescription { get; set; }

        public string ImageContent { get; set; }
    }
}
