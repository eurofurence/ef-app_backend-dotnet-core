using System.Collections.Generic;

namespace Eurofurence.App.Server.Web.Extensions
{
    public class OpenGraphMetadata
    {
        public Dictionary<string, string> Properties { get; private set; }
        public string Redirect { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }

        public OpenGraphMetadata()
        {
            Properties = new Dictionary<string, string>();
        }

        public OpenGraphMetadata WithProperty(string property, string value)
        {
            Properties.Add(property, value);
            return this;
        }
        public OpenGraphMetadata WithTitle(string value)
        {
            Title = value;
            Properties.Add("og:title", value);
            return this;
        }

        public OpenGraphMetadata WithDescription(string value)
        {
            Description = value;
            Properties.Add("og:description", value);
            return this;
        }

        public OpenGraphMetadata WithRedirect(string targetUrl)
        {
            Redirect = targetUrl;
            return this;
        }

        public OpenGraphMetadata WithImage(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl)) return this;

            ImageUrl = imageUrl;
            Properties.Add("og:image", imageUrl);
            return this;
        }
    }
}
