using System;

namespace Eurofurence.App.Server.Services.Abstractions.Sanitization
{
    public interface IHtmlSanitizer
    {
        /// <summary>
        /// Sanitizes input by stripping all HTML tags except for the optionally allowed ones, keeping the child nodes of stripped nodes.
        /// </summary>
        /// <param name="html">string to be sanitized</param>
        /// <returns>sanitized string</returns>
        string Sanitize(string html);
    }
}