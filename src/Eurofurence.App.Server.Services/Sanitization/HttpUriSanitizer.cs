using System;
using System.Text.RegularExpressions;
using Eurofurence.App.Server.Services.Abstractions.Sanitization;

namespace Eurofurence.App.Server.Services.Sanitization
{
    public class HttpUriSanitizer : IHttpUriSanitizer
    {
        private readonly Regex _uriMatcher;
        public HttpUriSanitizer()
        {
            _uriMatcher = new Regex(@"^http(s?):\/\/[^\/\.]+\.[^\/]+");
        }
        public string Sanitize(string uri, bool fixPrefix = true)
        {
            if (string.IsNullOrWhiteSpace(uri)) return null;

            if (!uri.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase) &&
                    !uri.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
                if (fixPrefix)
                    uri = $"https://{uri}";
                else
                    return null;

            if (_uriMatcher.IsMatch(uri) && Uri.IsWellFormedUriString(uri, UriKind.Absolute))
                return uri;
            else
                return null;
        }
    }
}