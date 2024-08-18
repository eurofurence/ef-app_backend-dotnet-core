using System;
using Eurofurence.App.Server.Services.Abstractions.Sanitization;

namespace Eurofurence.App.Server.Services.Sanitization
{
    public class UriSanitizer : IUriSanitizer
    {
        public Uri Sanitize(string uri, bool fixPrefix = true)
        {
            if (string.IsNullOrWhiteSpace(uri)) return null;

            if (!uri.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase) &&
                    !uri.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
                if (fixPrefix)
                    uri = $"https://{uri}";
                else
                    return null;

            Uri sanitizedUri;

            if (!Uri.TryCreate(uri, UriKind.Absolute, out sanitizedUri))
                return null;

            return sanitizedUri;
        }
    }
}