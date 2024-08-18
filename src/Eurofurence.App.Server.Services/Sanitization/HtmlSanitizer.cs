using System;
using Ganss.Xss;

namespace Eurofurence.App.Server.Services.Sanitization
{
    public class GanssHtmlSanitizer : Eurofurence.App.Server.Services.Abstractions.Sanitization.IHtmlSanitizer
    {
        private static readonly HtmlSanitizer _htmlSanitizer = new HtmlSanitizer();

        public GanssHtmlSanitizer()
        {
            _htmlSanitizer.AllowedTags.Clear();
            _htmlSanitizer.KeepChildNodes = true;
        }
        public string Sanitize(string html)
        {
            return _htmlSanitizer.Sanitize(html);
        }
    }
}