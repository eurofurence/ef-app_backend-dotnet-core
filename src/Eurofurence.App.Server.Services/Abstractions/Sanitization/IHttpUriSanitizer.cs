using System;

namespace Eurofurence.App.Server.Services.Abstractions.Sanitization
{
    public interface IHttpUriSanitizer
    {
        /// <summary>
        /// Attempts to sanitize given string as URI for an http(s) resource by optionally checking and fixing a missing <c>http://</c> or <c>https://</c> prefix by prepending <c>https://</c>
        /// </summary>
        /// <param name="uri">string to be sanitized</param>
        /// <param name="fixPrefix">Should missing schema prefix be fixed by prepending <c>https://</c>?</param>
        /// <returns>sanitized URI or <c>null</c> upon failure</returns>
        string Sanitize(string uri, bool fixPrefix = true);
    }
}